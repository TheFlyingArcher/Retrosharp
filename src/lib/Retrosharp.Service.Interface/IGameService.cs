using Retrosharp.Contract.Game;
using Retrosharp.Format;

namespace Retrosharp.Service.Interface
{
    /// <summary>
    /// Service interface for managing game-related business logic and ETL operations.
    /// </summary>
    public interface IGameService : IEntityService<Game>
    {
        /// <summary>
        /// Processes a collection of GameLog records from a Retrosheet file and saves them to the database.
        /// </summary>
        /// <param name="gameLogs">The collection of game log records to process.</param>
        /// <param name="seasonYear">The season year for the games.</param>
        /// <returns>The number of games successfully processed.</returns>
        Task<int> ProcessGameLogsAsync(IEnumerable<GameLog> gameLogs, int seasonYear);

        /// <summary>
        /// Gets all games for a specific franchise (home or visitor).
        /// </summary>
        /// <param name="franchiseId">The ID of the franchise.</param>
        /// <returns>A collection of games involving the franchise.</returns>
        Task<IEnumerable<Game>> GetByFranchiseIdAsync(int franchiseId);

        /// <summary>
        /// Gets games played on a specific date.
        /// </summary>
        /// <param name="gameDate">The date to search for.</param>
        /// <returns>A collection of games played on the specified date.</returns>
        Task<IEnumerable<Game>> GetByDateAsync(DateTime gameDate);
    }
}
