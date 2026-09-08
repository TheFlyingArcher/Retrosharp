namespace Retrosharp.Engine.Console.Saga
{
    public class GameLogSagaData : BaseSagaData
    {
        /// <summary>
        /// The season being imported, kept for logging when the saga completes.
        /// </summary>
        public int SeasonYear { get; set; }
    }
}
