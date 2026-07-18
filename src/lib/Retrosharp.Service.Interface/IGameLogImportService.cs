namespace Retrosharp.Service.Interface
{
    /// <summary>
    /// Parses a Retrosheet game log file and populates Game, GameLineup, and the three
    /// Game*Statistics tables, per spec/game-log.md. Distinct from IGameService, which covers
    /// read/lookup rather than bulk ETL import.
    /// </summary>
    public interface IGameLogImportService
    {
        /// <summary>
        /// Parses the game log file at the given path and inserts Game records (with their
        /// lineups and statistics) as a single atomic operation. Safe to call repeatedly --
        /// games already present, matched by their natural key, are skipped.
        /// </summary>
        Task<GameLogImportResult> ImportAsync(string filePath, int seasonYear);
    }
}
