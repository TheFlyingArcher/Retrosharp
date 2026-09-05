namespace Retrosharp.Message.GameEvent
{
    /// <summary>
    /// Signals that a per-file Game Event import belonging to a bulk run has failed and its
    /// <see cref="GameEventStart"/> message has been moved to the error queue. Emitted by the
    /// endpoint's <c>OnMessageSentToErrorQueue</c> hook in Retrosharp.Engine.Console (the
    /// per-file GameEventSaga deliberately does not report its own failures); handled by
    /// BulkGameEventImportSaga, correlated on <see cref="BulkImportId"/>, to mark the file
    /// failed and dispatch the next one. See spec/bulk-import.md, "A failed file must not
    /// stall the batch".
    /// </summary>
    public class GameEventImportFailed : BaseMessage, IMessage
    {
        public GameEventImportFailed() { }

        /// <summary>
        /// The bulk import run the failed file belonged to.
        /// </summary>
        public Guid BulkImportId { get; set; }

        /// <summary>
        /// The event file that failed, e.g. <c>2024SDN.EVN</c> (name only, not the full
        /// working-directory path).
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// A short summary of the failure (exception type and message) for the
        /// BulkImportFile row and the status endpoint.
        /// </summary>
        public string Error { get; set; }
    }
}
