using Retrosharp.Contract.Standing;

namespace Retrosharp.Service.Interface
{
    /// <summary>
    /// Computes and serves precomputed franchise-season standings. See
    /// spec/frontend-prototype.md's "Resolved: Standings Derivation" note.
    /// </summary>
    public interface IStandingsService
    {
        /// <summary>
        /// Recomputes and persists every participating franchise's standing for
        /// <paramref name="seasonYear"/> from that season's already-imported <c>Game</c> results.
        /// Safe to call repeatedly -- always replaces the season's rows wholesale rather than
        /// incrementally, so re-running it (for example, after importing more of that season's
        /// Game Log data) simply recomputes from whatever games exist now.
        /// </summary>
        /// <returns>The number of franchises a standing was computed for.</returns>
        Task<int> RecomputeSeasonAsync(short seasonYear);

        /// <summary>
        /// Gets one franchise's precomputed standing for one season, or null if it hasn't been
        /// computed (or the franchise didn't play that season).
        /// </summary>
        Task<FranchiseSeasonStanding> GetByFranchiseSeasonAsync(int franchiseId, short seasonYear);

        /// <summary>
        /// Gets every franchise's precomputed standing for one season, ordered by rank.
        /// </summary>
        Task<IEnumerable<FranchiseSeasonStanding>> GetBySeasonAsync(short seasonYear);
    }
}
