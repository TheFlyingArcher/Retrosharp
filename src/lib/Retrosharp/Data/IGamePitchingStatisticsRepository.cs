using System;
using System.Collections.Generic;
using System.Text;

using Retrosharp.Contract.Game;

namespace Retrosharp.Data
{
    /// <summary>
    /// Provides repository operations for team-level game pitching statistics,
    /// including retrieval by game, by franchise, and by franchise and game.
    /// </summary>
    public interface IGamePitchingStatisticsRepository : IRepository<GamePitchingStatistics>
    {
        /// <summary>
        /// Retrieves all pitching statistics for the specified game.
        /// </summary>
        Task<IEnumerable<GamePitchingStatistics>> GetByGameIdAsync(int gameId);

        /// <summary>
        /// Retrieves all pitching statistics for the specified franchise.
        /// </summary>
        Task<IEnumerable<GamePitchingStatistics>> GetByFranchiseIdAsync(int franchiseId);

        /// <summary>
        /// Retrieves all pitching statistics for the specified franchise and game.
        /// </summary>
        Task<IEnumerable<GamePitchingStatistics>> GetByFranchiseAndGameAsync(int franchiseId, int gameId);

        /// <summary>
        /// Sums TeamEarnedRuns across the given franchises' games in one season -- the
        /// authoritative team-earned figure used as the league-wide ERA numerator by
        /// <see cref="Format.PlayByPlay.FipConstantCalculator"/>. GamePitchingStatistics has no
        /// season column of its own, so this joins through Game for the season filter.
        /// </summary>
        /// <param name="franchiseIds">The franchises belonging to the league.</param>
        /// <param name="season">The season year.</param>
        Task<int> GetLeagueTeamEarnedRunsAsync(IEnumerable<int> franchiseIds, short season);
    }
}
