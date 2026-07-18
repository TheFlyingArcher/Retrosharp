namespace Retrosharp.Service.Interface
{
    /// <summary>
    /// Reports how many Game records were added versus skipped by a game log import.
    /// </summary>
    public class GameLogImportResult
    {
        public int GamesAdded { get; set; }

        public int GamesSkipped { get; set; }
    }
}
