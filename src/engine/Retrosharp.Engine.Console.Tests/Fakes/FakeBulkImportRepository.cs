using Retrosharp.Contract.BulkImport;
using Retrosharp.Data;

namespace Retrosharp.Engine.Console.Tests.Fakes
{
    /// <summary>
    /// Hand-rolled <see cref="IBulkImportRepository"/> double, matching this project's test
    /// conventions. Holds the single run created via <see cref="CreateAsync"/> and mutates its
    /// file rows in place as the saga marks them, so tests assert against
    /// <see cref="Run"/> directly.
    /// </summary>
    internal sealed class FakeBulkImportRepository : IBulkImportRepository
    {
        private readonly List<BulkImportFile> _files = new();
        private int _nextFileId = 1;

        /// <summary>Configurable prior outcomes for the rerun-skip lookup, keyed on (season, file name).</summary>
        public Dictionary<(short Season, string FileName), BulkImportFileStatus> PriorOutcomes { get; } = new();

        /// <summary>The run created by the saga (null until <see cref="CreateAsync"/> is called).</summary>
        public BulkImport? Run { get; private set; }

        public Task<BulkImport> CreateAsync(BulkImport bulkImport)
        {
            AssertStorable(bulkImport.CreatedUtc);
            AssertStorable(bulkImport.CompletedUtc);

            if (Run is not null && Run.TrackingId == bulkImport.TrackingId)
                return Task.FromResult(Run);

            _files.Clear();
            foreach (var file in bulkImport.Files)
            {
                _files.Add(new BulkImportFile
                {
                    Id = _nextFileId++,
                    BulkImportId = 1,
                    FileName = file.FileName,
                    Status = file.Status,
                    ErrorMessage = file.ErrorMessage,
                    GamesInserted = file.GamesInserted,
                    GamesSkipped = file.GamesSkipped
                });
            }

            Run = new BulkImport
            {
                Id = 1,
                TrackingId = bulkImport.TrackingId,
                SeasonYear = bulkImport.SeasonYear,
                SourceZipPath = bulkImport.SourceZipPath,
                WorkingDirectory = bulkImport.WorkingDirectory,
                BatchSize = bulkImport.BatchSize,
                Status = bulkImport.Status,
                FailureReason = bulkImport.FailureReason,
                CreatedUtc = bulkImport.CreatedUtc,
                CompletedUtc = bulkImport.CompletedUtc,
                Files = _files
            };

            return Task.FromResult(Run);
        }

        public Task<BulkImport?> GetByTrackingIdAsync(Guid trackingId) =>
            Task.FromResult(Run is not null && Run.TrackingId == trackingId ? Run : null);

        public Task<BulkImportFileStatus?> GetMostRecentFileOutcomeAsync(short seasonYear, string fileName) =>
            Task.FromResult(PriorOutcomes.TryGetValue((seasonYear, fileName), out var status) ? (BulkImportFileStatus?)status : null);

        public Task MarkFileInProgressAsync(int bulkImportFileId, DateTime startedUtc)
        {
            AssertStorable(startedUtc);
            var file = Find(bulkImportFileId);
            file.Status = BulkImportFileStatus.InProgress;
            file.StartedUtc = startedUtc;
            return Task.CompletedTask;
        }

        public Task MarkFileCompletedAsync(
            int bulkImportFileId,
            BulkImportFileStatus status,
            DateTime processedUtc,
            string? errorMessage = null,
            int? gamesInserted = null,
            int? gamesSkipped = null)
        {
            AssertStorable(processedUtc);
            var file = Find(bulkImportFileId);
            file.Status = status;
            file.ProcessedUtc = processedUtc;
            file.ErrorMessage = errorMessage;
            file.GamesInserted = gamesInserted;
            file.GamesSkipped = gamesSkipped;
            return Task.CompletedTask;
        }

        public Task UpdateBulkStatusAsync(
            int bulkImportId,
            BulkImportStatus status,
            string? failureReason = null,
            DateTime? completedUtc = null)
        {
            AssertStorable(completedUtc);
            Run!.Status = status;
            if (failureReason is not null)
                Run.FailureReason = failureReason;
            if (completedUtc is not null)
                Run.CompletedUtc = completedUtc;
            return Task.CompletedTask;
        }

        public BulkImportFile FileNamed(string fileName) =>
            _files.Single(f => f.FileName == fileName);

        private BulkImportFile Find(int id) => _files.Single(f => f.Id == id);

        // Mirrors what Npgsql enforces for this schema's "timestamp without time zone"
        // columns: a Kind=Utc DateTime is rejected. Catches a regression of the bug found
        // during Step 8's live run.
        private static void AssertStorable(DateTime? value)
        {
            if (value is { Kind: DateTimeKind.Utc })
                throw new Xunit.Sdk.XunitException(
                    $"DateTime {value} has Kind=Utc; Npgsql rejects that for a 'timestamp without time zone' column. Use DateTimeKind.Unspecified.");
        }
    }
}
