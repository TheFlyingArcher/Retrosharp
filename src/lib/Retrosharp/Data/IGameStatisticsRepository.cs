using System;
using System.Collections.Generic;
using System.Text;

using Retrosharp.Contract.Game;

namespace Retrosharp.Data
{
    /// <summary>
    /// Provides repository operations for baseball game statistics,
    /// including retrieval by game, by franchise, and by franchise and game.
    /// </summary>
    /// <remarks>Extends IRepository<GameStatistics>. Asynchronous retrieval methods return
    /// Task<IEnumerable<GameStatistics>> and yield an empty sequence when no statistics are found.</remarks>
    public interface IGameStatisticsRepository : IRepository<GameStatistics>
    {
        /// <summary>
        /// Retrieves all game statistics for the specified game.
        /// </summary>
        /// <remarks>Returns an empty sequence if no statistics are found for the game.</remarks>
        /// <param name="gameId">The identifier of the game.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable of GameStatistics
        /// for the specified game.</returns>
        Task<IEnumerable<GameStatistics>> GetByGameIdAsync(int gameId);

        /// <summary>
        /// Retrieves all game statistics for the specified franchise.
        /// </summary>
        /// <param name="franchiseId">The identifier of the franchise.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable of GameStatistics
        /// for the specified franchise.</returns>
        Task<IEnumerable<GameStatistics>> GetByFranchiseIdAsync(int franchiseId);

        /// <summary>
        /// Retrieves all game statistics for the specified franchise and game.
        /// </summary>
        /// <param name="franchiseId">The identifier of the franchise.</param>
        /// <param name="gameId">The identifier of the game.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable of GameStatistics
        /// for the specified franchise and game.</returns>
        Task<IEnumerable<GameStatistics>> GetByFranchiseAndGameAsync(int franchiseId, int gameId);
    }
}
