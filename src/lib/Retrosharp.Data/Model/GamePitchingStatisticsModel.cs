using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Retrosharp.Data.Model
{
    /// <summary>
    /// Represents aggregate team-level pitching statistics for one team in one game,
    /// sourced from the Game Log Parser. See spec/game-log.md, Format fields 39-43/67-71.
    /// </summary>
    [Table("GamePitchingStatistics")]
    public class GamePitchingStatisticsModel : DbModel
    {
        /// <summary>
        /// Foreign key to the game.
        /// </summary>
        [ForeignKey("Game")]
        [Required]
        public int GameId { get; set; }

        /// <summary>
        /// Foreign key to the franchise.
        /// </summary>
        [ForeignKey("Franchise")]
        [Required]
        public int FranchiseId { get; set; }

        /// <summary>
        /// Indicates whether stats are for home or visitor team ("H" or "V").
        /// </summary>
        [Required]
        [StringLength(1)]
        public string HomeVisitor { get; set; }

        /// <summary>
        /// Number of pitchers used (1 means it was a complete game).
        /// </summary>
        [Required]
        public byte PitchersUsed { get; set; }

        /// <summary>
        /// Total individual earned runs.
        /// </summary>
        [Required]
        public short IndividualEarnedRuns { get; set; }

        /// <summary>
        /// Total team earned runs.
        /// </summary>
        [Required]
        public short TeamEarnedRuns { get; set; }

        /// <summary>
        /// Total wild pitches.
        /// </summary>
        [Required]
        public byte WildPitches { get; set; }

        /// <summary>
        /// Total balks.
        /// </summary>
        [Required]
        public byte Balks { get; set; }

        // Navigation Properties

        /// <summary>
        /// Navigation property for the game.
        /// </summary>
        public GameModel Game { get; set; }

        /// <summary>
        /// Navigation property for the franchise.
        /// </summary>
        public FranchiseModel Franchise { get; set; }
    }
}
