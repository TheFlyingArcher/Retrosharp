using System;

namespace Retrosharp.Contract.GameEvent
{
    /// <summary>
    /// Represents one of Retrosheet's less common adjustment records (badj, padj, ladj, radj,
    /// presadj). These are infrequent enough that a single table with an AdjustmentType column
    /// is used rather than the full normalization applied to GameEvent.
    /// See spec/game-event.md, "Data Model" section.
    /// </summary>
    public class GameAdjustment : Entity
    {
        /// <summary>
        /// Foreign key to the game.
        /// </summary>
        public int GameId { get; set; }

        /// <summary>
        /// Order of this adjustment among adjustments only.
        /// </summary>
        public int Sequence { get; set; }

        /// <summary>
        /// Position of this record within the game's full Retrosheet record list -- shared
        /// across GameEvent/GameSubstitution/GameAdjustment/GameComment, used to interleave
        /// all four in true chronological order. See <see cref="GameEvent.RecordIndex"/>.
        /// </summary>
        public int RecordIndex { get; set; }

        /// <summary>
        /// The type of adjustment record.
        /// </summary>
        public GameAdjustmentType AdjustmentType { get; set; }

        /// <summary>
        /// Foreign key to the person this adjustment applies to.
        /// </summary>
        public int PersonId { get; set; }

        /// <summary>
        /// The adjustment's value (for example, a handedness code, or a lineup position),
        /// interpreted according to <see cref="AdjustmentType"/>.
        /// </summary>
        public string Value { get; set; }
    }
}
