namespace Retrosharp.Message.GameEvent
{
    public class GameEventCancel : BaseMessage, IMessage
    {
        public GameEventCancel() { }

        /// <summary>
        /// The file path of the Retrosheet play-by-play event file whose import is cancelled.
        /// </summary>
        public string FilePath { get; set; }
    }
}
