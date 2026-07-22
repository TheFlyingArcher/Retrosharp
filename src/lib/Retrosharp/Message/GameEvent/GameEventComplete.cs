namespace Retrosharp.Message.GameEvent
{
    public class GameEventComplete : BaseMessage, IMessage
    {
        public GameEventComplete() { }

        /// <summary>
        /// The file path of the Retrosheet play-by-play event file that was processed.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Number of games' play-by-play inserted into GameEvent.
        /// </summary>
        public int GamesInserted { get; set; }

        /// <summary>
        /// Number of games skipped because their play-by-play was already present.
        /// </summary>
        public int GamesSkipped { get; set; }

        /// <summary>
        /// Number of games whose Batting/Pitching/Fielding statistics were newly applied.
        /// </summary>
        public int StatisticsApplied { get; set; }

        /// <summary>
        /// Number of games whose statistics were already claimed by a prior run.
        /// </summary>
        public int StatisticsSkipped { get; set; }
    }
}
