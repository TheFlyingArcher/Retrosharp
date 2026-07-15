using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Retrosharp.Data.Model
{
    /// <summary>
    /// Represents aggregate team-level offensive statistics for one team in one game,
    /// sourced from the Game Log Parser. See spec/game-log.md.
    /// </summary>
    [Table("GameBattingStatistics")]
    public class GameBattingStatisticsModel : DbModel
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
        /// Total plate appearances.
        /// </summary>
        [Required]
        public short PlateAppearances { get; set; }

        /// <summary>
        /// Total at-bats.
        /// </summary>
        [Required]
        public short AtBats { get; set; }

        /// <summary>
        /// Total hits.
        /// </summary>
        [Required]
        public short Hit { get; set; }

        /// <summary>
        /// Total doubles.
        /// </summary>
        [Required]
        public short Doubles { get; set; }

        /// <summary>
        /// Total triples.
        /// </summary>
        [Required]
        public short Triples { get; set; }

        /// <summary>
        /// Total home runs.
        /// </summary>
        [Required]
        public short Homeruns { get; set; }

        /// <summary>
        /// Total runs batted in (RBIs).
        /// </summary>
        [Required]
        public short RunsBattedIn { get; set; }

        /// <summary>
        /// Total bases on balls (walks).
        /// </summary>
        [Required]
        public short BaseOnBalls { get; set; }

        /// <summary>
        /// Total strikeouts.
        /// </summary>
        [Required]
        public short Strikeouts { get; set; }

        /// <summary>
        /// Total sacrifice flies.
        /// </summary>
        [Required]
        public short SacrificeFlies { get; set; }

        /// <summary>
        /// Total sacrifice bunts.
        /// </summary>
        [Required]
        public short SacrificeBunts { get; set; }

        /// <summary>
        /// Total intentional bases on balls.
        /// </summary>
        [Required]
        public short IntentionalBb { get; set; }

        /// <summary>
        /// Total hit-by-pitches.
        /// </summary>
        [Required]
        public short HitByPitches { get; set; }

        /// <summary>
        /// Total stolen bases.
        /// </summary>
        [Required]
        public short StolenBases { get; set; }

        /// <summary>
        /// Total times caught stealing.
        /// </summary>
        [Required]
        public short TimesCaughtStealing { get; set; }

        /// <summary>
        /// Total runs scored.
        /// </summary>
        [Required]
        public short Runs { get; set; }

        /// <summary>
        /// Total grounded into double plays.
        /// </summary>
        [Required]
        public short GroundedIntoDoublePlay { get; set; }

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
