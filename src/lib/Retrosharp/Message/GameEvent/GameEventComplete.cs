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
        /// The bulk import run this file belonged to, copied through from
        /// <see cref="GameEventStart.BulkImportId"/>. <see cref="System.Guid.Empty"/> for a
        /// standalone single-file import. When set, the bulk import saga also handles this
        /// message, correlated on this value, to record the file's success. See
        /// spec/bulk-import.md.
        /// </summary>
        public Guid BulkImportId { get; set; }

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
