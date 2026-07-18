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
        /// Retrieves the franchise era whose franchise code was in effect on the given date.
        /// FranchiseCode alone is not unique -- Retrosheet reuses the same code across
        /// consecutive eras of the same franchise -- so callers that need to resolve a code from
        /// a dated source record (for example, a Game Log row) should use this instead of
        /// <see cref="GetByFranchiseCodeAsync"/>, which would otherwise pick an arbitrary era.
        /// </summary>
        /// <param name="franchiseCode">The franchise code to search for.</param>
        /// <param name="asOfDate">The date the code needs to have been in effect on.</param>
        /// <returns>The franchise era with the specified code active on the given date, or null if not found.</returns>
        Task<Franchise> GetByFranchiseCodeAndDateAsync(string franchiseCode, DateTime asOfDate);

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
