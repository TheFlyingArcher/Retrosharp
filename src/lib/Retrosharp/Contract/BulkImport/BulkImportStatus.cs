namespace Retrosharp.Contract.BulkImport
{
    /// <summary>
    /// Overall state of one bulk Game Event import run. See spec/bulk-import.md, "Data Model".
    /// </summary>
    public enum BulkImportStatus
    {
        /// <summary>
        /// The run has been recorded but the saga has not started processing files yet.
        /// </summary>
        Pending,

        /// <summary>
        /// Files are being dispatched and processed.
        /// </summary>
        InProgress,

        /// <summary>
        /// Every file ended <see cref="BulkImportFileStatus.Success"/> or
        /// <see cref="BulkImportFileStatus.Skipped"/>.
        /// </summary>
        Completed,

        /// <summary>
        /// The run finished, but at least one file ended <see cref="BulkImportFileStatus.Failed"/>.
        /// </summary>
        CompletedWithFailures,

        /// <summary>
        /// The run never started processing files -- a prerequisite or archive problem (the
        /// season's Game Log is not imported, the archive is unreadable, it spans multiple
        /// seasons, or it contains no event files). <see cref="Retrosharp.Contract.BulkImport.BulkImport.FailureReason"/>
        /// carries the detail.
        /// </summary>
        Failed
    }
}
