using System;
using System.Collections.Generic;
using System.Text;

using Retrosharp.Contract.Game;

namespace Retrosharp.Data
{
    /// <summary>
    /// A repository which provides data access methods for GameLineup entities,
    /// allowing retrieval of starting lineups for specific games.
    /// </summary>
    public interface IGameLineupRepository : IRepository<GameLineup>
    {
        /// <summary>
        /// Get the starting lineup for a given game.
        /// This will include both the home and visiting teams, with the visiting team listed first.
        /// The lineup will be ordered by the LineupOrder property, which indicates the batting order for each player.
        /// The HomeVisitor property will indicate whether the player is on the home team or the visiting team.
        /// </summary>
        /// <param name="gameId">The ID of the game played</param>
        /// <returns>A list of game lineups matching the given game ID</returns>
        Task<IEnumerable<GameLineup>> GetByGameIdAsync(int gameId);
    }
}
