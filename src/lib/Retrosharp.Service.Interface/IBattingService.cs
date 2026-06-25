using Retrosharp.Contract.Batting;

namespace Retrosharp.Service.Interface
{
    /// <summary>
    /// Service interface for managing batting statistics business logic.
    /// </summary>
    public interface IBattingService : IEntityService<Batting>
    {
        /// <summary>
        /// Gets all batting statistics for a specific person.
        /// </summary>
        /// <param name="personId">The ID of the person.</param>
        /// <returns>A collection of batting statistics for the person.</returns>
        Task<IEnumerable<Batting>> GetByPersonIdAsync(int personId);

        /// <summary>
        /// Gets batting statistics for a specific person, franchise, and season combination.
        /// </summary>
        /// <param name="personId">The ID of the person.</param>
        /// <param name="franchiseId">The ID of the franchise.</param>
        /// <param name="seasonYear">The season year.</param>
        /// <returns>The batting statistics for the specified combination.</returns>
        Task<Batting> GetByPersonFranchiseSeasonAsync(int personId, int franchiseId, short seasonYear);
    }
}
