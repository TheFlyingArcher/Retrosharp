namespace Retrosharp.Message.GameLog
{
    public class GameLogStart : BaseMessage, IMessage
    {
        public GameLogStart() { }

        /// <summary>
        /// The baseball season whose game log is to be downloaded from Retrosheet and
        /// processed. See spec/retrosheet-auto-download.md.
        /// </summary>
        public int SeasonYear { get; set; }
    }
}
