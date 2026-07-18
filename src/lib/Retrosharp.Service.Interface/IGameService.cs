using Retrosharp.Contract.Game;

namespace Retrosharp.Service.Interface
{
    /// <summary>
    /// Service interface for managing game-related read/lookup operations. Bulk ETL import is
    /// handled by IGameLogImportService instead.
    /// </summary>
    public interface IGameService : IEntityService<Game>
    {
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
