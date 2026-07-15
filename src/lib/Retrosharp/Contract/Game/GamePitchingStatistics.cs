using System;

namespace Retrosharp.Contract.Game
{
    /// <summary>
    /// Represents aggregate team-level pitching statistics for one team in one game,
    /// sourced from the Game Log Parser. See spec/game-log.md, Format fields 39-43/67-71.
    /// </summary>
    public class GamePitchingStatistics : Entity
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
        /// Number of pitchers used (1 means it was a complete game).
        /// </summary>
        public byte PitchersUsed { get; set; }

        /// <summary>
        /// Total individual earned runs.
        /// </summary>
        public short IndividualEarnedRuns { get; set; }

        /// <summary>
        /// Total team earned runs.
        /// </summary>
        public short TeamEarnedRuns { get; set; }

        /// <summary>
        /// Total wild pitches.
        /// </summary>
        public byte WildPitches { get; set; }

        /// <summary>
        /// Total balks.
        /// </summary>
        public byte Balks { get; set; }
    }
}
