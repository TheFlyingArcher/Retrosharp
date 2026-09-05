using Retrosharp.Contract.BulkImport;

namespace Retrosharp.Data
{
    /// <summary>
    /// Persists the state of bulk Game Event import runs (<see cref="BulkImport"/>) and their
    /// per-file rows (<see cref="BulkImportFile"/>). Written by the bulk import saga in
    /// Retrosharp.Engine.Console as it dispatches and resolves each file; read by
    /// <c>GET /api/gameevent/bulkimport/{trackingId}</c>. See spec/bulk-import.md.
    /// </summary>
    public interface IBulkImportRepository
    {
        /// <summary>
        /// Inserts a bulk import run together with every discovered file row (already carrying
        /// its initial <see cref="BulkImportFile.Status"/> of <c>Pending</c> or <c>Skipped</c>)
        /// as a single transaction, and returns it with database-assigned <c>Id</c>s populated
        /// on it and every file. Idempotent on <see cref="BulkImport.TrackingId"/>: if a run
        /// with that id already exists it is returned unchanged, so a retried start handler
        /// converges instead of violating the unique index.
        /// </summary>
        Task<BulkImport> CreateAsync(BulkImport bulkImport);

        /// <summary>
        /// Gets a bulk import run and its files by the caller-facing tracking id, or null if no
        /// run has that id.
        /// </summary>
        Task<BulkImport?> GetByTrackingIdAsync(Guid trackingId);

        /// <summary>
        /// Gets the <see cref="BulkImportFile.Status"/> of the most recently created row for
        /// the given season and file name, across every prior run, or null if the file has
        /// never been seen for that season. Drives the rerun skip decision -- a most-recent
        /// outcome of <see cref="BulkImportFileStatus.Success"/> means the file is skipped.
        /// </summary>
        Task<BulkImportFileStatus?> GetMostRecentFileOutcomeAsync(short seasonYear, string fileName);

        /// <summary>
        /// Marks one file row <see cref="BulkImportFileStatus.InProgress"/> and records when it
        /// was dispatched.
        /// </summary>
        Task MarkFileInProgressAsync(int bulkImportFileId, DateTime startedUtc);

        /// <summary>
        /// Records a file row's terminal outcome -- <see cref="BulkImportFileStatus.Success"/>
        /// or <see cref="BulkImportFileStatus.Failed"/> -- with the per-file game counts (on
        /// success) or the exception summary (on failure).
        /// </summary>
        Task MarkFileCompletedAsync(
            int bulkImportFileId,
            BulkImportFileStatus status,
            DateTime processedUtc,
            string? errorMessage = null,
            int? gamesInserted = null,
            int? gamesSkipped = null);

        /// <summary>
        /// Updates the run-level status, optionally recording a <paramref name="failureReason"/>
        /// (for <see cref="BulkImportStatus.Failed"/>) and a <paramref name="completedUtc"/>.
        /// </summary>
        Task UpdateBulkStatusAsync(
            int bulkImportId,
            BulkImportStatus status,
            string? failureReason = null,
            DateTime? completedUtc = null);
    }
}
