namespace Retrosharp.Message.GameEvent
{
    /// <summary>
    /// Starts a bulk Game Event import: download a season's zip archive of team-season event
    /// files from Retrosheet and orchestrate the existing per-file Game Event saga over them
    /// in batches. Placed on the bus by <c>POST /api/gameevent/bulkimport</c>; started by
    /// BulkGameEventImportSaga in Retrosharp.Engine.Console. See spec/bulk-import.md and
    /// spec/retrosheet-auto-download.md.
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
        /// The season to import. Always populated by the controller (which range-checks it);
        /// the engine builds the Retrosheet event-archive URL from it and cross-checks it
        /// against the season encoded in the downloaded archive's file names. Nullable only so
        /// a malformed message is caught as a validation failure rather than a deserialization
        /// error.
        /// </summary>
        public int? SeasonYear { get; set; }

        /// <summary>
        /// Optional. Maximum number of files processed concurrently. Falls back to the
        /// configured default (10) when omitted.
        /// </summary>
        public int? BatchSize { get; set; }
    }
}
