using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Retrosharp.Data.Model
{
    /// <summary>
    /// Represents season batting statistics for a player on a franchise.
    /// </summary>
    [Table("Batting")]
    public class BattingModel : DbModel
    {
        /// <summary>
        /// Foreign key to the person (player).
        /// </summary>
        [ForeignKey("Person")]
        [Required]
        public int PersonId { get; set; }

        /// <summary>
        /// Foreign key to the franchise.
        /// </summary>
        [ForeignKey("FranchiseModel")]
        [Required]
        public int FranchiseId { get; set; }

        /// <summary>
        /// Season year for these statistics.
        /// </summary>
        public short? SeasonYear { get; set; }

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
        public short Hits { get; set; }

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
        /// Number of positions played.
        /// </summary>
        [Required]
        public short Positions { get; set; }

        /// <summary>
        /// Total grounded into double plays.
        /// </summary>
        [Required]
        public short GroundedIntoDoublePlay { get; set; }

        // Navigation Properties

        /// <summary>
        /// Navigation property for the person (player).
        /// </summary>
        public PersonModel Person { get; set; }

        /// <summary>
        /// Navigation property for the franchise.
        /// </summary>
        public FranchiseModel Franchise { get; set; }
    }
}
