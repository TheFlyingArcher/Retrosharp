using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Retrosharp.Data.Model
{
    /// <summary>
    /// Represents a player's appearance in a game's batting lineup.
    /// </summary>
    [Table("GameLineup")]
    public class GameLineupModel : DbModel
    {
        /// <summary>
        /// Foreign key to the game.
        /// </summary>
        [ForeignKey("GameModel")]
        [Required]
        public int GameId { get; set; }

        /// <summary>
        /// Indicates whether the batter played for home or visitor team ("H" or "V").
        /// </summary>
        [Required]
        [StringLength(1)]
        public string HomeVisitor { get; set; }

        /// <summary>
        /// Batting order position (1-9).
        /// </summary>
        [Required]
        public byte LineupOrder { get; set; }

        /// <summary>
        /// Foreign key to the batter.
        /// </summary>
        [ForeignKey("Batter")]
        [Required]
        public int BatterId { get; set; }

        /// <summary>
        /// Defensive position played (e.g., "1B", "SS", "CF").
        /// </summary>
        [StringLength(3)]
        public string Position { get; set; }

        // Navigation Properties

        /// <summary>
        /// Navigation property for the game.
        /// </summary>
        public GameModel Game { get; set; }

        /// <summary>
        /// Navigation property for the batter.
        /// </summary>
        public PersonModel Batter { get; set; }
    }
}
