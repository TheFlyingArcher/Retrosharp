using System;
using System.Collections.Generic;
using System.Text;

using Retrosharp.Contract.Game;

namespace Retrosharp.Data
{
    /// <summary>
    /// Provides repository operations for team-level game fielding statistics,
    /// including retrieval by game, by franchise, and by franchise and game.
    /// </summary>
    public interface IGameFieldingStatisticsRepository : IRepository<GameFieldingStatistics>
    {
        /// <summary>
        /// Retrieves all fielding statistics for the specified game.
        /// </summary>
        Task<IEnumerable<GameFieldingStatistics>> GetByGameIdAsync(int gameId);

        /// <summary>
        /// Retrieves all fielding statistics for the specified franchise.
        /// </summary>
        Task<IEnumerable<GameFieldingStatistics>> GetByFranchiseIdAsync(int franchiseId);

        /// <summary>
        /// Retrieves all fielding statistics for the specified franchise and game.
        /// </summary>
        Task<IEnumerable<GameFieldingStatistics>> GetByFranchiseAndGameAsync(int franchiseId, int gameId);

        /// <summary>
        /// Retrieves a franchise's team fielding statistics for every game in one season --
        /// joins through Game since GameFieldingStatistics has no season column of its own.
        /// </summary>
        Task<IEnumerable<GameFieldingStatistics>> GetByFranchiseSeasonAsync(int franchiseId, short season);
    }
}
