using Microsoft.Extensions.Logging;
using NServiceBus;

using Retrosharp.Configuration;
using Retrosharp.Contract.BulkImport;
using Retrosharp.Data;
using Retrosharp.Message.GameEvent;
using Retrosharp.Service.Interface.ETL;

using ContractBulkImport = Retrosharp.Contract.BulkImport.BulkImport;

namespace Retrosharp.Engine.Console.Saga
{
    /// <summary>
    /// Orchestrates a bulk Game Event import: extracts a season's zip archive of team-season
    /// event files, validates the season's Game Log is imported, records a per-file status
    /// row for each, then drives the existing <see cref="GameEventSaga"/> over them -- one
    /// <c>GameEventStart</c> per file, at most <see cref="BulkGameEventImportSagaData.BatchSize"/>
    /// in flight. Per-file parsing/idempotency/atomicity/reconciliation is entirely
    /// <see cref="GameEventSaga"/>'s; this saga only sequences the work, tracks status,
    /// and cleans up. See spec/bulk-import.md.
    /// </summary>
    public class BulkGameEventImportSaga : Saga<BulkGameEventImportSagaData>,
        IAmStartedByMessages<BulkGameEventImportStart>,
        IHandleMessages<GameEventComplete>,
        IHandleMessages<GameEventImportFailed>,
        IHandleTimeouts<BulkGameEventImportSaga.Watchdog>
    {
        private readonly ILogger<BulkGameEventImportSaga> _logger;
        private readonly IBulkImportRepository _bulkImportRepository;
        private readonly IGameRepository _gameRepository;
        private readonly IRetrosheetArchiveClient _archiveClient;
        private readonly BulkImportConfiguration _configuration;
        private readonly RetrosheetSourceConfiguration _source;

        public BulkGameEventImportSaga(
            ILogger<BulkGameEventImportSaga> logger,
            IBulkImportRepository bulkImportRepository,
            IGameRepository gameRepository,
            IRetrosheetArchiveClient archiveClient,
            BulkImportConfiguration configuration,
            RetrosheetSourceConfiguration source)
        {
            _logger = logger;
            _bulkImportRepository = bulkImportRepository;
            _gameRepository = gameRepository;
            _archiveClient = archiveClient;
            _configuration = configuration;
            _source = source;
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<BulkGameEventImportSagaData> mapper)
        {
            // Correlated on the run's tracking id, which every child message carries. A
            // GameEventComplete from a standalone single-file import carries Guid.Empty and
            // simply finds no bulk saga instance (it is still handled by GameEventSaga).
            mapper.MapSaga(s => s.BulkImportId)
                .ToMessage<BulkGameEventImportStart>(m => m.BulkImportId)
                .ToMessage<GameEventComplete>(m => m.BulkImportId)
                .ToMessage<GameEventImportFailed>(m => m.BulkImportId);
        }

        public async Task Handle(BulkGameEventImportStart message, IMessageHandlerContext context)
        {
            if (Data.ProcessingStarted)
            {
                _logger.LogInformation(
                    "Bulk Game Event import {BulkImportId} is already running; ignoring duplicate start.",
                    message.BulkImportId);
                return;
            }

            var batchSize = message.BatchSize is > 0 ? message.BatchSize.Value : _configuration.DefaultBatchSize;
            var workingDirectory = ResolveWorkingDirectory(message.BulkImportId);

            // --- Validation: any failure here records the run as Failed and stops, with the
            // reason visible on GET /api/gameevent/bulkimport/{trackingId}. The one exception
            // is a transient download failure (RetrosheetArchiveUnavailableException) -- it is
            // left to propagate so the message is retried; no BulkImport row is written until
            // the archive is in hand and validated. ---

            if (message.SeasonYear is not { } requestedSeason || !EventFileArchive.IsPlausibleSeason(requestedSeason))
            {
                await FailStartupAsync(context, message, 0, workingDirectory,
                    "No valid season year was supplied for the bulk import.");
                return;
            }
            var season = (short)requestedSeason;

            string archivePath;
            try
            {
                archivePath = await _archiveClient.DownloadAsync(
                    _source.EventArchiveUri(season), workingDirectory, context.CancellationToken);
            }
            catch (RetrosheetArchiveNotFoundException ex)
            {
                await FailStartupAsync(context, message, season, workingDirectory,
                    $"The event archive for season {season} could not be downloaded: {ex.Message}");
                return;
            }
            catch (InvalidDataException ex)
            {
                await FailStartupAsync(context, message, season, workingDirectory,
                    $"The event archive downloaded for season {season} is not a valid zip: {ex.Message}");
                return;
            }

            IReadOnlyList<string> archiveFiles;
            try
            {
                archiveFiles = EventFileArchive.ListEventFiles(archivePath);
            }
            catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException or InvalidDataException)
            {
                await FailStartupAsync(context, message, season, workingDirectory,
                    $"The downloaded event archive for season {season} could not be read: {ex.Message}");
                return;
            }

            if (!EventFileArchive.TryResolveSeason(archiveFiles, out var archiveSeason, out var seasonError))
            {
                await FailStartupAsync(context, message, season, workingDirectory, seasonError!);
                return;
            }

            if (archiveSeason != season)
            {
                await FailStartupAsync(context, message, season, workingDirectory,
                    $"The request specified season {season}, but the downloaded archive's event files are for {archiveSeason}.");
                return;
            }

            var gameLogImported = (await _gameRepository.GetBySeasonAsync(season)).Any();
            if (!gameLogImported)
            {
                await FailStartupAsync(context, message, season, workingDirectory,
                    $"The Game Log for season {season} has not been imported. Import it first, then retry the bulk import.");
                return;
            }

            // --- Seed: decide skip-vs-process per file, extract the ones to process, persist. ---

            var seeds = new List<BulkImportFile>(archiveFiles.Count);
            var toProcess = new List<string>();
            foreach (var fileName in archiveFiles)
            {
                // A most-recent outcome of Success OR Skipped both mean "this file's games
                // are already imported for this season" -- a later run that skipped it does
                // not un-import it. Only Failed (or no prior row) warrants reprocessing.
                var priorOutcome = await _bulkImportRepository.GetMostRecentFileOutcomeAsync(season, fileName);
                var status = priorOutcome is BulkImportFileStatus.Success or BulkImportFileStatus.Skipped
                    ? BulkImportFileStatus.Skipped
                    : BulkImportFileStatus.Pending;

                if (status == BulkImportFileStatus.Pending)
                    toProcess.Add(fileName);

                seeds.Add(new BulkImportFile { FileName = fileName, Status = status });
            }

            EventFileArchive.ExtractFiles(archivePath, workingDirectory, toProcess);

            var run = await _bulkImportRepository.CreateAsync(new ContractBulkImport
            {
                TrackingId = message.BulkImportId,
                SeasonYear = season,
                SourceZipPath = _source.EventArchiveUri(season).ToString(),
                WorkingDirectory = workingDirectory,
                BatchSize = batchSize,
                Status = BulkImportStatus.InProgress,
                CreatedUtc = UtcNow,
                Files = seeds
            });

            Data.BulkImportId = message.BulkImportId;
            Data.BulkImportRowId = run.Id;
            Data.SeasonYear = season;
            Data.WorkingDirectory = workingDirectory;
            Data.DownloadedArchivePath = archivePath;
            Data.BatchSize = batchSize;
            Data.ProcessingStarted = true;
            Data.Files = run.Files
                .Select(f => new BulkGameEventImportFileState { Id = f.Id, FileName = f.FileName, Status = f.Status })
                .ToList();

            var skipped = Data.Files.Count(f => f.Status == BulkImportFileStatus.Skipped);
            _logger.LogInformation(
                "Bulk Game Event import {BulkImportId} for season {SeasonYear}: {Total} file(s) discovered, {Skipped} already imported (skipped), {ToProcess} to process, batch size {BatchSize}.",
                message.BulkImportId, season, Data.Files.Count, skipped, toProcess.Count, batchSize);

            if (Data.Files.All(f => IsTerminal(f.Status)))
            {
                await FinishAsync(context);
                return;
            }

            await RequestTimeout(context, TimeSpan.FromHours(_configuration.WatchdogTimeoutHours),
                new Watchdog { BulkImportId = message.BulkImportId });
            await DispatchAsync(context);
        }

        public async Task Handle(GameEventComplete message, IMessageHandlerContext context)
        {
            await ResolveFileAsync(context, Path.GetFileName(message.FilePath), BulkImportFileStatus.Success,
                error: null, gamesInserted: message.GamesInserted, gamesSkipped: message.GamesSkipped);
        }

        public async Task Handle(GameEventImportFailed message, IMessageHandlerContext context)
        {
            await ResolveFileAsync(context, message.FileName, BulkImportFileStatus.Failed,
                error: message.Error, gamesInserted: null, gamesSkipped: null);
        }

        public async Task Timeout(Watchdog state, IMessageHandlerContext context)
        {
            var unfinished = Data.Files.Where(f => !IsTerminal(f.Status)).ToList();
            if (unfinished.Count == 0)
                return;

            _logger.LogWarning(
                "Bulk Game Event import {BulkImportId} watchdog fired with {Count} file(s) still unfinished; marking them Failed.",
                Data.BulkImportId, unfinished.Count);

            var now = UtcNow;
            foreach (var file in unfinished)
            {
                file.Status = BulkImportFileStatus.Failed;
                await _bulkImportRepository.MarkFileCompletedAsync(file.Id, BulkImportFileStatus.Failed, now,
                    "No completion signal was received before the bulk import watchdog timeout; the file may still be on the error queue.");
            }

            await FinishAsync(context);
        }

        private async Task ResolveFileAsync(
            IMessageHandlerContext context,
            string fileName,
            BulkImportFileStatus outcome,
            string? error,
            int? gamesInserted,
            int? gamesSkipped)
        {
            var file = Data.Files.FirstOrDefault(f =>
                string.Equals(f.FileName, fileName, StringComparison.OrdinalIgnoreCase));

            if (file is null || file.Status != BulkImportFileStatus.InProgress)
            {
                // A late or duplicate signal -- the watchdog already failed this file, or it
                // isn't one of ours. Nothing to do.
                return;
            }

            file.Status = outcome;
            await _bulkImportRepository.MarkFileCompletedAsync(
                file.Id, outcome, UtcNow, error, gamesInserted, gamesSkipped);

            if (outcome == BulkImportFileStatus.Failed)
                _logger.LogWarning("Bulk Game Event import {BulkImportId}: '{FileName}' failed -- {Error}",
                    Data.BulkImportId, fileName, error);

            await DispatchAsync(context);

            if (Data.Files.All(f => IsTerminal(f.Status)))
                await FinishAsync(context);
        }

        private async Task DispatchAsync(IMessageHandlerContext context)
        {
            var inFlight = Data.Files.Count(f => f.Status == BulkImportFileStatus.InProgress);
            var now = UtcNow;

            while (inFlight < Data.BatchSize)
            {
                var next = Data.Files.FirstOrDefault(f => f.Status == BulkImportFileStatus.Pending);
                if (next is null)
                    break;

                next.Status = BulkImportFileStatus.InProgress;
                await _bulkImportRepository.MarkFileInProgressAsync(next.Id, now);
                await context.SendLocal(new GameEventStart
                {
                    RequestId = Guid.NewGuid(),
                    FilePath = Path.Combine(Data.WorkingDirectory, next.FileName),
                    BulkImportId = Data.BulkImportId
                });

                inFlight++;
            }
        }

        private async Task FinishAsync(IMessageHandlerContext context)
        {
            var succeeded = Data.Files.Count(f => f.Status == BulkImportFileStatus.Success);
            var failed = Data.Files.Count(f => f.Status == BulkImportFileStatus.Failed);
            var skipped = Data.Files.Count(f => f.Status == BulkImportFileStatus.Skipped);

            foreach (var file in Data.Files.Where(f => f.Status == BulkImportFileStatus.Success))
                TryDelete(Path.Combine(Data.WorkingDirectory, file.FileName));

            // The downloaded archive is always removed -- it is re-downloadable, and keeping it
            // would defeat the "temp directory, cleaned up after the run" contract. Failed
            // event files are still left in place for investigation.
            TryDelete(Data.DownloadedArchivePath);

            TryRemoveEmptyDirectory(Data.WorkingDirectory);

            var status = failed > 0 ? BulkImportStatus.CompletedWithFailures : BulkImportStatus.Completed;
            await _bulkImportRepository.UpdateBulkStatusAsync(Data.BulkImportRowId, status, failureReason: null, completedUtc: UtcNow);

            _logger.Log(failed > 0 ? LogLevel.Warning : LogLevel.Information,
                "Bulk Game Event import {BulkImportId} for season {SeasonYear} {Status}: {Succeeded} succeeded, {Failed} failed, {Skipped} skipped (of {Total}).",
                Data.BulkImportId, Data.SeasonYear, status, succeeded, failed, skipped, Data.Files.Count);

            MarkAsComplete();
        }

        private async Task FailStartupAsync(
            IMessageHandlerContext context,
            BulkGameEventImportStart message,
            short season,
            string workingDirectory,
            string reason)
        {
            _logger.LogWarning(
                "Bulk Game Event import {BulkImportId} rejected: {Reason}", message.BulkImportId, reason);

            var now = UtcNow;
            await _bulkImportRepository.CreateAsync(new ContractBulkImport
            {
                TrackingId = message.BulkImportId,
                SeasonYear = season,
                SourceZipPath = season > 0 ? _source.EventArchiveUri(season).ToString() : string.Empty,
                WorkingDirectory = workingDirectory,
                BatchSize = message.BatchSize is > 0 ? message.BatchSize.Value : _configuration.DefaultBatchSize,
                Status = BulkImportStatus.Failed,
                FailureReason = reason,
                CreatedUtc = now,
                CompletedUtc = now,
                Files = Array.Empty<BulkImportFile>()
            });

            // The run never really started; a partly-downloaded archive is of no use to
            // anyone. Remove the whole working directory.
            WorkingDirectory.TryDelete(workingDirectory, _logger);

            MarkAsComplete();
        }

        private static bool IsTerminal(BulkImportFileStatus status) =>
            status is BulkImportFileStatus.Success or BulkImportFileStatus.Failed or BulkImportFileStatus.Skipped;

        // Every DateTime column in this schema is "timestamp without time zone", which Npgsql
        // will not write a Kind=Utc value into. Match the rest of the codebase (see
        // GameStatisticsRepository) and hand it Kind=Unspecified wall-clock UTC.
        private static DateTime UtcNow => DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        private string ResolveWorkingDirectory(Guid trackingId) =>
            Path.Combine(_source.ResolvedWorkingRoot, trackingId.ToString("N"));

        private void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Bulk Game Event import {BulkImportId}: could not delete '{Path}' during cleanup.",
                    Data.BulkImportId, path);
            }
        }

        private void TryRemoveEmptyDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path) && !Directory.EnumerateFileSystemEntries(path).Any())
                    Directory.Delete(path);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Bulk Game Event import {BulkImportId}: could not remove working directory '{Path}'.",
                    Data.BulkImportId, path);
            }
        }

        /// <summary>
        /// Saga timeout: the backstop for files that never send back a completion or failure
        /// signal (the engine crashing mid-file, a transient error retrying past the timeout).
        /// </summary>
        public class Watchdog
        {
            public Guid BulkImportId { get; set; }
        }
    }
}
