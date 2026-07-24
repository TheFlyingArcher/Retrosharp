using System;
using System.Collections.Generic;
using System.Text;

using Retrosharp.Contract.Game;

namespace Retrosharp.Data
{
    /// <summary>
    /// Provides repository operations for team-level game batting statistics,
    /// including retrieval by game, by franchise, and by franchise and game.
    /// </summary>
    public interface IGameBattingStatisticsRepository : IRepository<GameBattingStatistics>
    {
        /// <summary>
        /// Retrieves all batting statistics for the specified game.
        /// </summary>
        Task<IEnumerable<GameBattingStatistics>> GetByGameIdAsync(int gameId);

        /// <summary>
        /// Retrieves all batting statistics for the specified franchise.
        /// </summary>
        Task<IEnumerable<GameBattingStatistics>> GetByFranchiseIdAsync(int franchiseId);

        /// <summary>
        /// Retrieves all batting statistics for the specified franchise and game.
        /// </summary>
        Task<IEnumerable<GameBattingStatistics>> GetByFranchiseAndGameAsync(int franchiseId, int gameId);

        /// <summary>
        /// Retrieves a franchise's team batting statistics for every game in one season --
        /// joins through Game since GameBattingStatistics has no season column of its own.
        /// </summary>
        Task<IEnumerable<GameBattingStatistics>> GetByFranchiseSeasonAsync(int franchiseId, short season);
    }
}
