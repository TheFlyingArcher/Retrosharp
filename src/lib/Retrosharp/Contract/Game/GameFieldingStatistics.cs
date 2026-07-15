using System;

namespace Retrosharp.Contract.Game
{
    /// <summary>
    /// Represents aggregate team-level defensive statistics for one team in one game,
    /// sourced from the Game Log Parser. See spec/game-log.md, Format fields 44-49/72-77.
    /// </summary>
    public class GameFieldingStatistics : Entity
    {
        /// <summary>
        /// Foreign key to the game.
        /// </summary>
        public int GameId { get; set; }

        /// <summary>
        /// Foreign key to the franchise.
        /// </summary>
        public int FranchiseId { get; set; }

        /// <summary>
        /// Indicates whether stats are for home or visitor team ("H" or "V").
        /// </summary>
        public string HomeVisitor { get; set; }

        /// <summary>
        /// Total putouts.
        /// </summary>
        public short Putouts { get; set; }

        /// <summary>
        /// Total assists.
        /// </summary>
        public short Assists { get; set; }

        /// <summary>
        /// Total errors.
        /// </summary>
        public short Errors { get; set; }

        /// <summary>
        /// Total passed balls.
        /// </summary>
        public byte PassedBalls { get; set; }

        /// <summary>
        /// Total double plays.
        /// </summary>
        public byte DoublePlays { get; set; }

        /// <summary>
        /// Total triple plays.
        /// </summary>
        public byte TriplePlays { get; set; }
    }
}
