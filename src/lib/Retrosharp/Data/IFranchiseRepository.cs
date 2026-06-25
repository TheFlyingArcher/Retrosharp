using Retrosharp.Contract.Franchise;

namespace Retrosharp.Data
{
    /// <summary>
    /// Repository interface for managing Franchise entities in the data store.
    /// </summary>
    public interface IFranchiseRepository : IRepository<Franchise>
    {
        /// <summary>
        /// Retrieves a franchise by its franchise code.
        /// </summary>
        /// <param name="franchiseCode">The franchise code to search for.</param>
        /// <returns>The franchise with the specified code, or null if not found.</returns>
        Task<Franchise> GetByFranchiseCodeAsync(string franchiseCode);

        /// <summary>
        /// Retrieves all franchises for a specific league.
        /// </summary>
        /// <param name="leagueId">The ID of the league.</param>
        /// <returns>A collection of franchises in the specified league.</returns>
        Task<IEnumerable<Franchise>> GetByLeagueIdAsync(int leagueId);

        /// <summary>
        /// Retrieves all active franchises.
        /// </summary>
        /// <returns>A collection of all active franchises.</returns>
        Task<IEnumerable<Franchise>> GetActiveAsync();
    }
}
