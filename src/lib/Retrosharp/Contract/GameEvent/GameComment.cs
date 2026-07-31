using System;

namespace Retrosharp.Contract.GameEvent
{
    /// <summary>
    /// Represents a free-text commentary (com) record. Events without a dedicated Retrosheet
    /// record type, such as an ejection, are captured here for narrative context rather than
    /// as a structured event. See spec/game-event.md, "Data Model" section.
    /// </summary>
    public class GameComment : Entity
    {
        /// <summary>
        /// Foreign key to the game.
        /// </summary>
        public int GameId { get; set; }

        /// <summary>
        /// Order of this comment among comments only.
        /// </summary>
        public int Sequence { get; set; }

        /// <summary>
        /// Position of this record within the game's full Retrosheet record list -- shared
        /// across GameEvent/GameSubstitution/GameAdjustment/GameComment, used to interleave
        /// all four in true chronological order. See <see cref="GameEvent.RecordIndex"/>.
        /// </summary>
        public int RecordIndex { get; set; }

        /// <summary>
        /// The free-text comment.
        /// </summary>
        public string CommentText { get; set; }
    }
}
