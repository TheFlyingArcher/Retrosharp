using System;

namespace Retrosharp.Contract.Game
{
    /// <summary>
    /// Represents a player's appearance in a game's batting lineup.
    /// </summary>
    public class GameLineup : Entity
    {
        /// <summary>
        /// Foreign key to the game.
        /// </summary>
        public int GameId { get; set; }

        /// <summary>
        /// Indicates whether the batter played for home or visitor team ("H" or "V").
        /// </summary>
        public string HomeVisitor { get; set; }

        /// <summary>
        /// Batting order position (1-9).
        /// </summary>
        public byte LineupOrder { get; set; }

        /// <summary>
        /// Foreign key to the batter.
        /// </summary>
        public int BatterId { get; set; }

        /// <summary>
        /// Defensive position played (e.g., "1B", "SS", "CF").
        /// </summary>
        public string Position { get; set; }
    }
}
