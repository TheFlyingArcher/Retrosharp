namespace Retrosharp.Message.GameEvent
{
    public class GameEventStart : BaseMessage, IMessage
    {
        public GameEventStart() { }

        /// <summary>
        /// The file path of the Retrosheet play-by-play event file (.EVN/.EVA) to be processed.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// The bulk import run this file belongs to. Always set now that bulk import is the
        /// only path that produces a <see cref="GameEventStart"/> (the bulk saga sends one per
        /// extracted event file, via <c>SendLocal</c>). <see cref="System.Guid.Empty"/> only
        /// for a message constructed outside that flow. GameEventSaga persists it and echoes
        /// it back on <see cref="GameEventComplete"/> so the bulk import saga can correlate
        /// the outcome. See spec/bulk-import.md.
        /// </summary>
        public Guid BulkImportId { get; set; }
    }
}
