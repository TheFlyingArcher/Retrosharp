using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Retrosharp.Data.Model
{
    /// <summary>
    /// Represents a baseball franchise (team).
    /// </summary>
    [Table("Franchise")]
    public class FranchiseModel : DbModel
    {
        /// <summary>
        /// Foreign key to the League.
        /// </summary>
        [ForeignKey("League")]
        public int? LeagueId { get; set; }

        /// <summary>
        /// FranchiseModel identifier code.
        /// </summary>
        [Required]
        [StringLength(4)]
        public string FranchiseIdentifier { get; set; }

        /// <summary>
        /// FranchiseModel code (typically 3 letters).
        /// </summary>
        [Required]
        [StringLength(4)]
        public string FranchiseCode { get; set; }

        /// <summary>
        /// Division code (e.g., "AL", "NL").
        /// </summary>
        [StringLength(2)]
        public string DivisionCode { get; set; }

        /// <summary>
        /// Geographic location of the franchise.
        /// </summary>
        [Required]
        [StringLength(32)]
        public string FranchiseLocation { get; set; }

        /// <summary>
        /// Primary team nickname.
        /// </summary>
        [Required]
        [StringLength(64)]
        public string Nickname { get; set; }

        /// <summary>
        /// Alternative team nickname if applicable.
        /// </summary>
        [StringLength(64)]
        public string AlternateNickname { get; set; }

        /// <summary>
        /// Date when the franchise was established.
        /// </summary>
        [Required]
        public DateTime FranchiseStart { get; set; }

        /// <summary>
        /// Date when the franchise ceased operations (if applicable).
        /// </summary>
        public DateTime? FranchiseEnd { get; set; }

        /// <summary>
        /// City where the team plays/played their home games.
        /// </summary>
        [Required]
        [StringLength(32)]
        public string PlayingCity { get; set; }

        /// <summary>
        /// State abbreviation where the team plays/played.
        /// </summary>
        [Required]
        [StringLength(2)]
        public string PlayingState { get; set; }

        /// <summary>
        /// Indicates if the franchise is currently active.
        /// </summary>
        [Required]
        public bool IsActive { get; set; }

        /// <summary>
        /// Navigation property for the league this franchise belongs to.
        /// </summary>
        public LeagueModel League { get; set; }

        /// <summary>
        /// Navigation property for games where this franchise was the visitor.
        /// </summary>
        public ICollection<GameModel> VisitorGames { get; set; } = new List<GameModel>();

        /// <summary>
        /// Navigation property for games where this franchise was the home team.
        /// </summary>
        public ICollection<GameModel> HomeGames { get; set; } = new List<GameModel>();

        /// <summary>
        /// Navigation property for game statistics for this franchise.
        /// </summary>
        public ICollection<GameStatisticsModel> GameStatistics { get; set; } = new List<GameStatisticsModel>();

        /// <summary>
        /// Navigation property for player batting records with this franchise.
        /// </summary>
        public ICollection<BattingModel> BattingRecords { get; set; } = new List<BattingModel>();

        /// <summary>
        /// Navigation property for player pitching records with this franchise.
        /// </summary>
        public ICollection<PitchingModel> PitchingRecords { get; set; } = new List<PitchingModel>();
    }
}
