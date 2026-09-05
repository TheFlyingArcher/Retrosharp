using System;
using System.Collections.Generic;

namespace Retrosharp.Contract.BulkImport
{
    /// <summary>
    /// One bulk Game Event import request -- the unit the tracking identifier refers to.
    /// Owned entirely by the bulk import saga in Retrosharp.Engine.Console. See
    /// spec/bulk-import.md.
    /// </summary>
    public class BulkImport : Entity
    {
        /// <summary>
        /// The identifier returned to the caller and used by the status endpoint. Also the
        /// saga's <c>BulkImportId</c> correlation value.
        /// </summary>
        public Guid TrackingId { get; set; }

        /// <summary>
        /// Season the archive's event files belong to, parsed from their file names.
        /// </summary>
        public short SeasonYear { get; set; }

        /// <summary>
        /// The <c>.zip</c> path supplied in the request.
        /// </summary>
        public string SourceZipPath { get; set; } = string.Empty;

        /// <summary>
        /// Directory the archive was extracted into.
        /// </summary>
        public string WorkingDirectory { get; set; } = string.Empty;

        /// <summary>
        /// Effective number of files processed concurrently for this run.
        /// </summary>
        public int BatchSize { get; set; }

        public BulkImportStatus Status { get; set; }

        /// <summary>
        /// Set when <see cref="Status"/> is <see cref="BulkImportStatus.Failed"/> -- why the
        /// run never started processing files.
        /// </summary>
        public string? FailureReason { get; set; }

        public DateTime CreatedUtc { get; set; }

        public DateTime? CompletedUtc { get; set; }

        /// <summary>
        /// One row per event file discovered in the archive.
        /// </summary>
        public IReadOnlyList<BulkImportFile> Files { get; set; } = Array.Empty<BulkImportFile>();
    }
}
