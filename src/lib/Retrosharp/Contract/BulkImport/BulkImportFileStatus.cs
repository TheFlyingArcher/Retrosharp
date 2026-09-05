namespace Retrosharp.Contract.BulkImport
{
    /// <summary>
    /// State of one event file within a bulk Game Event import run. See spec/bulk-import.md,
    /// "Data Model".
    /// </summary>
    public enum BulkImportFileStatus
    {
        /// <summary>
        /// Discovered in the archive and waiting to be dispatched.
        /// </summary>
        Pending,

        /// <summary>
        /// A <c>GameEventStart</c> has been dispatched for this file and no result has come
        /// back yet.
        /// </summary>
        InProgress,

        /// <summary>
        /// The per-file Game Event import completed.
        /// </summary>
        Success,

        /// <summary>
        /// The per-file Game Event import failed; <see cref="Retrosharp.Contract.BulkImport.BulkImportFile.ErrorMessage"/>
        /// carries the exception summary and the child message is on the error queue.
        /// </summary>
        Failed,

        /// <summary>
        /// Not processed this run because its most recent prior outcome for the same season was
        /// <see cref="Success"/>. See spec/bulk-import.md, "Rerun skips files that already
        /// succeeded".
        /// </summary>
        Skipped
    }
}
