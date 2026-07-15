using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Retrosharp.Data.Model
{
    /// <summary>
    /// Represents aggregate team-level defensive statistics for one team in one game,
    /// sourced from the Game Log Parser. See spec/game-log.md, Format fields 44-49/72-77.
    /// </summary>
    [Table("GameFieldingStatistics")]
    public class GameFieldingStatisticsModel : DbModel
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
        /// Total putouts.
        /// </summary>
        [Required]
        public short Putouts { get; set; }

        /// <summary>
        /// Total assists.
        /// </summary>
        [Required]
        public short Assists { get; set; }

        /// <summary>
        /// Total errors.
        /// </summary>
        [Required]
        public short Errors { get; set; }

        /// <summary>
        /// Total passed balls.
        /// </summary>
        [Required]
        public byte PassedBalls { get; set; }

        /// <summary>
        /// Total double plays.
        /// </summary>
        [Required]
        public byte DoublePlays { get; set; }

        /// <summary>
        /// Total triple plays.
        /// </summary>
        [Required]
        public byte TriplePlays { get; set; }

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
