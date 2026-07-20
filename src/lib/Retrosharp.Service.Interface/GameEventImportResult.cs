namespace Retrosharp.Service.Interface
{
    /// <summary>
    /// Reports how many games' play-by-play were inserted versus skipped (already present) by
    /// a Game Event import.
    /// </summary>
    public class GameEventImportResult
    {
        public int GamesInserted { get; set; }

        public int GamesSkipped { get; set; }
    }
}
