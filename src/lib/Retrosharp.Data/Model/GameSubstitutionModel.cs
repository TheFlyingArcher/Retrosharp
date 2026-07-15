using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Retrosharp.Data.Model
{
    /// <summary>
    /// Represents a player entering the game mid-game (position player substitution, pinch
    /// hitter, or pinch runner). Modeled separately from GameEvent since it is not a play.
    /// See spec/game-event.md, "Data Model" section.
    /// </summary>
    [Table("GameSubstitution")]
    public class GameSubstitutionModel : DbModel
    {
        /// <summary>
        /// Foreign key to the game.
        /// </summary>
        [ForeignKey("Game")]
        [Required]
        public int GameId { get; set; }

        /// <summary>
        /// Order of this substitution within the game.
        /// </summary>
        [Required]
        public int Sequence { get; set; }

        /// <summary>
        /// Foreign key to the person entering the game.
        /// </summary>
        [ForeignKey("Person")]
        [Required]
        public int PersonId { get; set; }

        /// <summary>
        /// Indicates whether the substitution is for the home or visitor team ("H" or "V").
        /// </summary>
        [Required]
        [StringLength(1)]
        public string TeamAtBat { get; set; }

        /// <summary>
        /// Batting order position (1-9), or the position used for pinch hitter/runner
        /// designations per Retrosheet's convention.
        /// </summary>
        [Required]
        public byte BattingOrderPosition { get; set; }

        /// <summary>
        /// Defensive position (1-9), or 11/12 for pinch hitter/pinch runner.
        /// </summary>
        [Required]
        public byte FieldingPosition { get; set; }

        // Navigation Properties

        /// <summary>
        /// Navigation property for the game.
        /// </summary>
        public GameModel Game { get; set; }

        /// <summary>
        /// Navigation property for the person entering the game.
        /// </summary>
        public PersonModel Person { get; set; }
    }
}
