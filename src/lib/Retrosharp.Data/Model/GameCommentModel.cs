using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Retrosharp.Data.Model
{
    /// <summary>
    /// Represents a free-text commentary (com) record. Events without a dedicated Retrosheet
    /// record type, such as an ejection, are captured here for narrative context rather than
    /// as a structured event. See spec/game-event.md, "Data Model" section.
    /// </summary>
    [Table("GameComment")]
    public class GameCommentModel : DbModel
    {
        /// <summary>
        /// Foreign key to the game.
        /// </summary>
        [ForeignKey("Game")]
        [Required]
        public int GameId { get; set; }

        /// <summary>
        /// Order of this comment within the game.
        /// </summary>
        [Required]
        public int Sequence { get; set; }

        /// <summary>
        /// The free-text comment.
        /// </summary>
        [Required]
        [StringLength(2048)]
        public string CommentText { get; set; }

        // Navigation Properties

        /// <summary>
        /// Navigation property for the game.
        /// </summary>
        public GameModel Game { get; set; }
    }
}
