namespace Retrosharp.Service.Interface
{
    /// <summary>
    /// Reports how many games' play-by-play were inserted versus skipped (already present),
    /// and how many games' statistics were newly applied versus already claimed by a prior
    /// run, by a Game Event import.
    /// </summary>
    public class GameEventImportResult
    {
        public int GamesInserted { get; set; }

        public int GamesSkipped { get; set; }

        public int StatisticsApplied { get; set; }

        public int StatisticsSkipped { get; set; }
    }
}
