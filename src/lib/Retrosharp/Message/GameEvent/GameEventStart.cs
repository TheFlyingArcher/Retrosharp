namespace Retrosharp.Message.GameEvent
{
    public class GameEventStart : BaseMessage, IMessage
    {
        public GameEventStart() { }

        /// <summary>
        /// The file path of the Retrosheet play-by-play event file (.EVN/.EVA) to be processed.
        /// </summary>
        public string FilePath { get; set; }
    }
}
