namespace Retrosharp.Message.GameEvent
{
    /// <summary>
    /// Starts a bulk Game Event import: extract a season's zip archive of team-season event
    /// files and orchestrate the existing per-file Game Event saga over them in batches.
    /// Placed on the bus by <c>POST /api/gameevent/bulkimport</c>; started by
    /// BulkGameEventImportSaga in Retrosharp.Engine.Console. See spec/bulk-import.md.
    /// </summary>
    public class BulkGameEventImportStart : BaseMessage, IMessage
    {
        public BulkGameEventImportStart() { }

        /// <summary>
        /// The caller-facing tracking identifier for this run, also the bulk saga's
        /// correlation key and the <see cref="GameEventStart.BulkImportId"/> stamped on every
        /// child message.
        /// </summary>
        public Guid BulkImportId { get; set; }

        /// <summary>
        /// Path to the <c>.zip</c> archive of event files, on a volume visible to both
        /// Retrosharp.UI.Api and Retrosharp.Engine.Console.
        /// </summary>
        public string ZipPath { get; set; }

        /// <summary>
        /// Optional. When supplied, validated against the season parsed from the archive's
        /// file names; a mismatch fails the run. When omitted, the season is taken from the
        /// file names.
        /// </summary>
        public int? SeasonYear { get; set; }

        /// <summary>
        /// Optional. Maximum number of files processed concurrently. Falls back to the
        /// configured default (10) when omitted.
        /// </summary>
        public int? BatchSize { get; set; }
    }
}
