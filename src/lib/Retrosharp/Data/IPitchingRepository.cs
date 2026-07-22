using System;
using System.Collections.Generic;
using System.Text;
using Retrosharp.Contract.Pitching;

namespace Retrosharp.Data
{
    /// <summary>
    /// Repository interface for managing Pitching statistics entities in the data store.
    /// </summary>
    public interface IPitchingRepository : IRepository<Pitching>
    {
        /// <summary>
        /// Retrieves all pitching statistics for a specific person.
        /// </summary>
        /// <param name="personId">The ID of the person whose pitching statistics to retrieve.</param>
        /// <returns>A collection of pitching statistics for the specified person.</returns>
        Task<IEnumerable<Pitching>> GetByPersonIdAsync(int personId);

        /// <summary>
        /// Retrieves pitching statistics for a specific person, franchise, and season.
        /// </summary>
        /// <param name="personId">The ID of the person.</param>
        /// <param name="franchiseId">The ID of the franchise.</param>
        /// <param name="seasonYear">The season year.</param>
        /// <returns>The pitching statistics for the specified combination, or null if not found.</returns>
        Task<Pitching> GetByPersonFranchiseSeasonAsync(int personId, int franchiseId, short seasonYear);

        /// <summary>
        /// Sums BaseOnBalls/HitBatsmen/Strikeouts/InningsPitched across every pitcher at the
        /// given franchises for one season -- the league-wide input to
        /// <see cref="Format.PlayByPlay.FipConstantCalculator"/>. See spec/api.md,
        /// "LeagueSeasonPitchingTotals".
        /// </summary>
        /// <param name="franchiseIds">The franchises belonging to the league.</param>
        /// <param name="season">The season year.</param>
        Task<(int BaseOnBalls, int HitBatsmen, int Strikeouts, int InningsPitchedOuts)> GetLeagueTotalsAsync(IEnumerable<int> franchiseIds, short season);
    }
}
