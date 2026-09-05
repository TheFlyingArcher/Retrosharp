namespace Retrosharp.Message.GameEvent
{
    public class GameEventCancel : BaseMessage, IMessage
    {
        public GameEventCancel() { }

        /// <summary>
        /// The file path of the Retrosheet play-by-play event file whose import is cancelled.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// The bulk import run this file belonged to, copied through from
        /// <see cref="GameEventStart.BulkImportId"/>. <see cref="System.Guid.Empty"/> for a
        /// standalone single-file import. Carried for symmetry with
        /// <see cref="GameEventComplete"/>. See spec/bulk-import.md.
        /// </summary>
        public Guid BulkImportId { get; set; }
    }
}
