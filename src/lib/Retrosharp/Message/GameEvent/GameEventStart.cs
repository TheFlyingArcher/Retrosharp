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
        /// The bulk import run this file belongs to, when the import was initiated by
        /// <see cref="BulkGameEventImportStart"/>. <see cref="System.Guid.Empty"/> for a
        /// standalone single-file import through <c>POST /api/gameevent/import</c>. When set,
        /// GameEventSaga persists it and echoes it back on <see cref="GameEventComplete"/>
        /// so the bulk import saga can correlate the outcome. See spec/bulk-import.md.
        /// </summary>
        public Guid BulkImportId { get; set; }
    }
}
