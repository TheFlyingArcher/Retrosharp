using System;
using System.Collections.Generic;
using System.Text;
using Retrosharp.Contract.Batting;

namespace Retrosharp.Data
{
    /// <summary>
    /// Repository interface for managing Batting statistics entities in the data store.
    /// </summary>
    public interface IBattingRepository : IRepository<Batting>
    {
        /// <summary>
        /// Retrieves all batting statistics for a specific person.
        /// </summary>
        /// <param name="personId">The ID of the person whose batting statistics to retrieve.</param>
        /// <returns>A collection of batting statistics for the specified person.</returns>
        Task<IEnumerable<Batting>> GetByPersonIdAsync(int personId);

        /// <summary>
        /// Retrieves batting statistics for a specific person, franchise, and season.
        /// </summary>
        /// <param name="personId">The ID of the person.</param>
        /// <param name="franchiseId">The ID of the franchise.</param>
        /// <param name="seasonYear">The season year.</param>
        /// <returns>The batting statistics for the specified combination, or null if not found.</returns>
        Task<Batting> GetByPersonFranchiseSeasonAsync(int personId, int franchiseId, short seasonYear);
    }
}
