using Microsoft.Extensions.Logging;
using NServiceBus;

using Retrosharp.Configuration;
using Retrosharp.Message.GameLog;
using Retrosharp.Service.Interface;
using Retrosharp.Service.Interface.ETL;

namespace Retrosharp.Engine.Console.Saga
{
    public class GameLogSaga : Saga<GameLogSagaData>,
        IAmStartedByMessages<GameLogStart>,
        IHandleMessages<GameLogComplete>,
        IHandleMessages<GameLogCancel>
    {
        private readonly ILogger<GameLogSaga> _logger;
        private readonly IGameLogImportService _gameLogImportService;
        private readonly IStandingsService _standingsService;
        private readonly IRetrosheetArchiveClient _archiveClient;
        private readonly RetrosheetSourceConfiguration _source;

        public GameLogSaga(
            ILogger<GameLogSaga> logger,
            IGameLogImportService gameLogImportService,
            IStandingsService standingsService,
            IRetrosheetArchiveClient archiveClient,
            RetrosheetSourceConfiguration source)
        {
            _logger = logger;
            _gameLogImportService = gameLogImportService;
            _standingsService = standingsService;
            _archiveClient = archiveClient;
            _source = source;
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<GameLogSagaData> mapper)
        {
            mapper.MapSaga(s => s.RequestId)
                .ToMessage<GameLogStart>(m => m.RequestId)
                .ToMessage<GameLogComplete>(m => m.RequestId)
                .ToMessage<GameLogCancel>(m => m.RequestId);
        }

        public async Task Handle(GameLogStart message, IMessageHandlerContext context)
        {
            Data.SeasonYear = message.SeasonYear;

            var archiveUrl = _source.GameLogUri(message.SeasonYear);
            var workingDirectory = Path.Combine(
                _source.ResolvedWorkingRoot, "gamelog", message.RequestId.ToString("N"));

            _logger.LogInformation(
                "Starting Game Log import for season {SeasonYear} from {ArchiveUrl}.",
                message.SeasonYear, archiveUrl);

            try
            {
                var archivePath = await _archiveClient.DownloadAsync(archiveUrl, workingDirectory, context.CancellationToken);
                var gameLogPath = GameLogArchive.Extract(archivePath, workingDirectory);

                // Any failure propagates to the endpoint's recoverability policy
                // (EngineRecoverabilityPolicy): a transient error (a 5xx/timeout download, a
                // dropped DB connection) is retried with backoff; an unrecoverable one (a 404
                // for a season Retrosheet has no data for, a corrupt archive, a deterministic
                // resolution failure) is routed straight to the error queue with no retries.
                // The saga deliberately does not catch-and-complete unrecoverable failures --
                // doing so left a failed import invisible (see spec/defects.md, "Needless
                // Retrying").
                var result = await _gameLogImportService.ImportAsync(gameLogPath, message.SeasonYear);

                // Final step: the Game rows are committed, so refresh the season's
                // precomputed standings. Failure here is reported on GameLogComplete but
                // deliberately never thrown -- see RecomputeStandingsAsync.
                var standings = await RecomputeStandingsAsync(message.SeasonYear);

                await context.SendLocal(new GameLogComplete
                {
                    RequestId = message.RequestId,
                    SeasonYear = message.SeasonYear,
                    GamesAdded = result.GamesAdded,
                    GamesSkipped = result.GamesSkipped,
                    StandingsRecomputed = standings.Recomputed,
                    StandingsFranchiseCount = standings.FranchiseCount,
                    StandingsError = standings.Error
                });
            }
            finally
            {
                // The working directory (downloaded zip + extracted game log) is scratch
                // space; a retry re-downloads. Remove it whether the import succeeded or
                // threw. See spec/retrosheet-auto-download.md.
                WorkingDirectory.TryDelete(workingDirectory, _logger);
            }
        }

        /// <summary>
        /// Recomputes the season's <c>FranchiseSeasonStanding</c> rows. The import itself has
        /// already committed by now, so a standings error must not fail the message: letting it
        /// propagate would retry (and re-download and re-parse) an import that succeeded, then
        /// land it on the error queue, hiding a good import behind a derived-data problem.
        /// Instead the error is logged and carried on <see cref="GameLogComplete"/>; standings
        /// are idempotent and can be rebuilt with <c>POST /api/standings/compute?season=</c>.
        /// Cancellation (host shutdown) is not swallowed, so the message is simply redelivered.
        /// </summary>
        private async Task<(bool Recomputed, int FranchiseCount, string? Error)> RecomputeStandingsAsync(int seasonYear)
        {
            try
            {
                var franchiseCount = await _standingsService.RecomputeSeasonAsync(checked((short)seasonYear));
                _logger.LogInformation(
                    "Recomputed standings for season {SeasonYear}: {FranchiseCount} franchise(s).",
                    seasonYear, franchiseCount);
                return (true, franchiseCount, null);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex,
                    "Game Log for season {SeasonYear} imported, but recomputing its standings failed. Re-run POST /api/standings/compute?season={SeasonYear} to retry.",
                    seasonYear, seasonYear);
                return (false, 0, $"{ex.GetType().Name}: {ex.Message}");
            }
        }

        public Task Handle(GameLogComplete message, IMessageHandlerContext context)
        {
            _logger.LogInformation(
                "Game Log import complete for season {SeasonYear}: {GamesAdded} added, {GamesSkipped} skipped; standings {StandingsOutcome}.",
                Data.SeasonYear, message.GamesAdded, message.GamesSkipped,
                message.StandingsRecomputed ? $"recomputed for {message.StandingsFranchiseCount} franchise(s)" : $"NOT recomputed ({message.StandingsError})");

            MarkAsComplete();
            return Task.CompletedTask;
        }

        public Task Handle(GameLogCancel message, IMessageHandlerContext context)
        {
            _logger.LogWarning(
                "Game Log import for season {SeasonYear} was cancelled.", Data.SeasonYear);

            MarkAsComplete();
            return Task.CompletedTask;
        }
    }
}
