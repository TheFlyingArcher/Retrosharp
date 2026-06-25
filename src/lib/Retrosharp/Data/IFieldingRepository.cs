using Retrosharp.Contract.Fielding;

namespace Retrosharp.Data
{
    /// <summary>
    /// Repository interface for managing Fielding statistics entities in the data store.
    /// </summary>
    public interface IFieldingRepository : IRepository<Fielding>
    {
        /// <summary>
        /// Retrieves all fielding statistics for a specific person.
        /// </summary>
        /// <param name="personId">The ID of the person whose fielding statistics to retrieve.</param>
        /// <returns>A collection of fielding statistics for the specified person.</returns>
        Task<IEnumerable<Fielding>> GetByPersonIdAsync(int personId);

        /// <summary>
        /// Retrieves fielding statistics for a specific person, franchise, and season.
        /// </summary>
        /// <param name="personId">The ID of the person.</param>
        /// <param name="franchiseId">The ID of the franchise.</param>
        /// <param name="seasonYear">The season year.</param>
        /// <returns>The fielding statistics for the specified combination, or null if not found.</returns>
        Task<Fielding> GetByPersonFranchiseSeasonAsync(int personId, int franchiseId, short seasonYear);
    }
}
