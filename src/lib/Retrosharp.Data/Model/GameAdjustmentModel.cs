using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Retrosharp.Contract.GameEvent;

namespace Retrosharp.Data.Model
{
    /// <summary>
    /// Represents one of Retrosheet's less common adjustment records (badj, padj, ladj, radj,
    /// presadj). These are infrequent enough that a single table with an AdjustmentType column
    /// is used rather than the full normalization applied to GameEvent.
    /// See spec/game-event.md, "Data Model" section.
    /// </summary>
    [Table("GameAdjustment")]
    public class GameAdjustmentModel : DbModel
    {
        /// <summary>
        /// Foreign key to the game.
        /// </summary>
        [ForeignKey("Game")]
        [Required]
        public int GameId { get; set; }

        /// <summary>
        /// Order of this adjustment among adjustments only.
        /// </summary>
        [Required]
        public int Sequence { get; set; }

        /// <summary>
        /// Position of this record within the game's full Retrosheet record list -- shared
        /// across GameEvent/GameSubstitution/GameAdjustment/GameComment, used to interleave
        /// all four in true chronological order. See GameEventModel.RecordIndex.
        /// </summary>
        [Required]
        public int RecordIndex { get; set; }

        /// <summary>
        /// The type of adjustment record.
        /// </summary>
        [Required]
        public GameAdjustmentType AdjustmentType { get; set; }

        /// <summary>
        /// Foreign key to the person this adjustment applies to.
        /// </summary>
        [ForeignKey("Person")]
        [Required]
        public int PersonId { get; set; }

        /// <summary>
        /// The adjustment's value (for example, a handedness code, or a lineup position),
        /// interpreted according to <see cref="AdjustmentType"/>.
        /// </summary>
        [StringLength(32)]
        public string Value { get; set; }

        // Navigation Properties

        /// <summary>
        /// Navigation property for the game.
        /// </summary>
        public GameModel Game { get; set; }

        /// <summary>
        /// Navigation property for the person this adjustment applies to.
        /// </summary>
        public PersonModel Person { get; set; }
    }
}
