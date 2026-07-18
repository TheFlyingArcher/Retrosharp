namespace Retrosharp.Engine.Console.Saga
{
    public class GameLogSagaData : BaseSagaData
    {
        public int SeasonYear { get; set; }

        /// <summary>
        /// The game log file path being processed, kept for logging when the saga completes.
        /// </summary>
        public string FilePath { get; set; }
    }
}
