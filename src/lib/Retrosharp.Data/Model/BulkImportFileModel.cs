using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Retrosharp.Contract.BulkImport;

namespace Retrosharp.Data.Model
{
    /// <summary>
    /// One event file discovered in a bulk Game Event import's archive, and its outcome. See
    /// spec/bulk-import.md, "Data Model".
    /// </summary>
    [Table("BulkImportFile")]
    public class BulkImportFileModel : DbModel
    {
        /// <summary>
        /// Foreign key to the owning <see cref="BulkImportModel"/>.
        /// </summary>
        [ForeignKey("BulkImport")]
        [Required]
        public int BulkImportId { get; set; }

        /// <summary>
        /// Event file name, e.g. <c>2024SDN.EVN</c>.
        /// </summary>
        [Required]
        [StringLength(64)]
        public string FileName { get; set; }

        [Required]
        public BulkImportFileStatus Status { get; set; }

        /// <summary>
        /// Exception summary when <see cref="Status"/> is <see cref="BulkImportFileStatus.Failed"/>;
        /// null otherwise.
        /// </summary>
        [StringLength(2048)]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Games inserted by the per-file import, from its <c>GameEventComplete</c>.
        /// </summary>
        public int? GamesInserted { get; set; }

        /// <summary>
        /// Games skipped by the per-file import (already present), from its <c>GameEventComplete</c>.
        /// </summary>
        public int? GamesSkipped { get; set; }

        public DateTime? StartedUtc { get; set; }

        public DateTime? ProcessedUtc { get; set; }

        // Navigation Properties

        /// <summary>
        /// Navigation property for the owning bulk import.
        /// </summary>
        public BulkImportModel BulkImport { get; set; }
    }
}
