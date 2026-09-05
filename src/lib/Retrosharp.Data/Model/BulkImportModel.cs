using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Retrosharp.Contract.BulkImport;

namespace Retrosharp.Data.Model
{
    /// <summary>
    /// One bulk Game Event import request. Owned entirely by the bulk import saga in
    /// Retrosharp.Engine.Console -- never written to by any parser. See spec/bulk-import.md,
    /// "Data Model".
    /// </summary>
    [Table("BulkImport")]
    public class BulkImportModel : DbModel
    {
        /// <summary>
        /// The identifier returned to the caller and used by the status endpoint; also the
        /// bulk saga's correlation value. Unique.
        /// </summary>
        [Required]
        public Guid TrackingId { get; set; }

        /// <summary>
        /// Season the archive's event files belong to, parsed from their file names.
        /// </summary>
        [Required]
        public short SeasonYear { get; set; }

        /// <summary>
        /// The <c>.zip</c> path supplied in the request.
        /// </summary>
        [Required]
        [StringLength(1024)]
        public string SourceZipPath { get; set; }

        /// <summary>
        /// Directory the archive was extracted into.
        /// </summary>
        [StringLength(1024)]
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Effective number of files processed concurrently for this run.
        /// </summary>
        [Required]
        public int BatchSize { get; set; }

        [Required]
        public BulkImportStatus Status { get; set; }

        /// <summary>
        /// Set when <see cref="Status"/> is <see cref="BulkImportStatus.Failed"/> -- why the
        /// run never started processing files. Null on every other status.
        /// </summary>
        [StringLength(1024)]
        public string? FailureReason { get; set; }

        [Required]
        public DateTime CreatedUtc { get; set; }

        public DateTime? CompletedUtc { get; set; }

        // Navigation Properties

        /// <summary>
        /// One row per event file discovered in the archive.
        /// </summary>
        public ICollection<BulkImportFileModel> Files { get; set; } = new List<BulkImportFileModel>();
    }
}
