using System;

namespace Retrosharp.Contract.BulkImport
{
    /// <summary>
    /// One event file discovered in a bulk Game Event import's archive, and its outcome. See
    /// spec/bulk-import.md, "Data Model".
    /// </summary>
    public class BulkImportFile : Entity
    {
        /// <summary>
        /// Foreign key to the owning <see cref="BulkImport"/>.
        /// </summary>
        public int BulkImportId { get; set; }

        /// <summary>
        /// Event file name, e.g. <c>2024SDN.EVN</c>.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        public BulkImportFileStatus Status { get; set; }

        /// <summary>
        /// Exception summary when <see cref="Status"/> is <see cref="BulkImportFileStatus.Failed"/>.
        /// </summary>
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
    }
}
