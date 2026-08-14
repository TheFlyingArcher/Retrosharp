using Retrosharp.Contract.Standing;

namespace Retrosharp.Data
{
    /// <summary>
    /// Stores precomputed franchise-season standings. Unlike Batting/Pitching/Fielding's
    /// per-game incremental deltas, a season's standings can only be computed as a whole (rank
    /// and games-behind are relative to every other franchise in the same grouping), so this
    /// repository replaces a whole season's rows atomically rather than upserting row by row.
    /// See spec/frontend-prototype.md's "Resolved: Standings Derivation" note.
    /// </summary>
    public interface IFranchiseSeasonStandingRepository
    {
        /// <summary>
        /// Gets one franchise's standing for one season, or null if that season hasn't been
        /// computed (or the franchise didn't play that season).
        /// </summary>
        Task<FranchiseSeasonStanding> GetByFranchiseSeasonAsync(int franchiseId, short seasonYear);

        /// <summary>
        /// Gets every franchise's standing for one season.
        /// </summary>
        Task<IEnumerable<FranchiseSeasonStanding>> GetBySeasonAsync(short seasonYear);

        /// <summary>
        /// Gets every precomputed standing row that exists, across every franchise and season.
        /// Used to derive franchise all-time career summaries (see
        /// <see cref="Retrosharp.Format.Standings.FranchiseCareerSummaryResolver"/>) -- at Phase
        /// 1's data volumes (one row per franchise-era-season, at most a few thousand rows) this
        /// is cheap to load in full, the same reasoning already applied to franchise search
        /// ("a small, bounded reference set").
        /// </summary>
        Task<IEnumerable<FranchiseSeasonStanding>> GetAllAsync();

        /// <summary>
        /// Atomically replaces every standing row for <paramref name="seasonYear"/> with
        /// <paramref name="standings"/> -- idempotent by construction, since recomputing the same
        /// season from the same games always produces the same result regardless of how many
        /// times it's run.
        /// </summary>
        Task ReplaceSeasonAsync(short seasonYear, IEnumerable<FranchiseSeasonStanding> standings);
    }
}
