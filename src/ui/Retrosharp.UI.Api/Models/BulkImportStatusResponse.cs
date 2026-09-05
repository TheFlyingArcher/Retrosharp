namespace Retrosharp.UI.Api.Models
{
    /// <summary>
    /// Progress of one bulk Game Event import run. See spec/bulk-import.md,
    /// "GET /api/gameevent/bulkimport/{trackingId}".
    /// </summary>
    public class BulkImportStatusResponse
    {
        public Guid TrackingId { get; set; }

        public short SeasonYear { get; set; }

        /// <summary>
        /// <c>Pending</c>, <c>InProgress</c>, <c>Completed</c>, <c>CompletedWithFailures</c>,
        /// or <c>Failed</c> (the run never started -- see <see cref="FailureReason"/>).
        /// </summary>
        public string Status { get; set; } = string.Empty;

        public int BatchSize { get; set; }

        /// <summary>
        /// Why the run was rejected before processing any file; null otherwise.
        /// </summary>
        public string? FailureReason { get; set; }

        public DateTime CreatedUtc { get; set; }

        public DateTime? CompletedUtc { get; set; }

        public BulkImportCounts Counts { get; set; } = new();

        public IReadOnlyList<BulkImportFileLine> Files { get; set; } = Array.Empty<BulkImportFileLine>();
    }

    /// <summary>
    /// Per-status file tallies for a bulk import run.
    /// </summary>
    public class BulkImportCounts
    {
        public int Total { get; set; }

        public int Pending { get; set; }

        public int InProgress { get; set; }

        public int Success { get; set; }

        public int Failed { get; set; }

        public int Skipped { get; set; }
    }

    /// <summary>
    /// One event file within a bulk import run.
    /// </summary>
    public class BulkImportFileLine
    {
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// <c>Pending</c>, <c>InProgress</c>, <c>Success</c>, <c>Failed</c>, or <c>Skipped</c>.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        public int? GamesInserted { get; set; }

        public int? GamesSkipped { get; set; }

        /// <summary>
        /// Exception summary when <see cref="Status"/> is <c>Failed</c>; null otherwise.
        /// </summary>
        public string? ErrorMessage { get; set; }

        public DateTime? StartedUtc { get; set; }

        public DateTime? ProcessedUtc { get; set; }
    }
}
