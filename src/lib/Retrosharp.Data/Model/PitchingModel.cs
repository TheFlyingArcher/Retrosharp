using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Retrosharp.Data.Model
{
    /// <summary>
    /// Represents season pitching statistics for a player on a franchise.
    /// </summary>
    [Table("Pitching")]
    public class PitchingModel : DbModel
    {
        /// <summary>
        /// Foreign key to the person (pitcher).
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
        [Required]
        public short SeasonYear { get; set; }

        /// <summary>
        /// Position indicator (e.g., "P" for pitcher).
        /// </summary>
        [Required]
        [StringLength(2)]
        public string Position { get; set; }

        /// <summary>
        /// Total games pitched.
        /// </summary>
        [Required]
        public short GamesPitched { get; set; }

        /// <summary>
        /// Total games started.
        /// </summary>
        [Required]
        public short GamesStarted { get; set; }

        /// <summary>
        /// Total games finished.
        /// </summary>
        [Required]
        public short GamesFinished { get; set; }

        /// <summary>
        /// Total complete games.
        /// </summary>
        [Required]
        public short CompleteGames { get; set; }

        /// <summary>
        /// Total shutouts.
        /// </summary>
        [Required]
        public short Shutouts { get; set; }

        /// <summary>
        /// Total saves.
        /// </summary>
        [Required]
        public short Saves { get; set; }

        /// <summary>
        /// Total innings pitched.
        /// </summary>
        [Required]
        public short InningsPitched { get; set; }

        /// <summary>
        /// Total hits allowed.
        /// </summary>
        [Required]
        public short Hits { get; set; }

        /// <summary>
        /// Total runs allowed.
        /// </summary>
        [Required]
        public short Runs { get; set; }

        /// <summary>
        /// Total earned runs allowed.
        /// </summary>
        [Required]
        public short EarnedRuns { get; set; }

        /// <summary>
        /// Total bases on balls (walks) issued.
        /// </summary>
        [Required]
        public short BaseOnBalls { get; set; }

        /// <summary>
        /// Total strikeouts recorded.
        /// </summary>
        [Required]
        public short Strikeouts { get; set; }

        /// <summary>
        /// Total intentional bases on balls issued.
        /// </summary>
        [Required]
        public short IntentionalBb { get; set; }

        /// <summary>
        /// Total batters hit by pitches.
        /// </summary>
        [Required]
        public short HitBatsmen { get; set; }

        /// <summary>
        /// Total balks.
        /// </summary>
        [Required]
        public short Balks { get; set; }

        /// <summary>
        /// Total wild pitches.
        /// </summary>
        [Required]
        public short WildPitches { get; set; }

        // Navigation Properties

        /// <summary>
        /// Navigation property for the person (pitcher).
        /// </summary>
        public PersonModel Person { get; set; }

        /// <summary>
        /// Navigation property for the franchise.
        /// </summary>
        public FranchiseModel Franchise { get; set; }
    }
}
