using System;

namespace Retrosharp.Contract.GameEvent
{
    /// <summary>
    /// Represents a player entering the game mid-game (position player substitution, pinch
    /// hitter, or pinch runner). Modeled separately from GameEvent since it is not a play.
    /// See spec/game-event.md, "Data Model" section.
    /// </summary>
    public class GameSubstitution : Entity
    {
        /// <summary>
        /// Foreign key to the game.
        /// </summary>
        public int GameId { get; set; }

        /// <summary>
        /// Order of this substitution among substitutions only.
        /// </summary>
        public int Sequence { get; set; }

        /// <summary>
        /// Position of this record within the game's full Retrosheet record list -- shared
        /// across GameEvent/GameSubstitution/GameAdjustment/GameComment, used to interleave
        /// all four in true chronological order. See <see cref="GameEvent.RecordIndex"/>.
        /// </summary>
        public int RecordIndex { get; set; }

        /// <summary>
        /// Foreign key to the person entering the game.
        /// </summary>
        public int PersonId { get; set; }

        /// <summary>
        /// Indicates whether the substitution is for the home or visitor team ("H" or "V").
        /// </summary>
        public string TeamAtBat { get; set; }

        /// <summary>
        /// Batting order position (1-9), or the position used for pinch hitter/runner
        /// designations per Retrosheet's convention.
        /// </summary>
        public byte BattingOrderPosition { get; set; }

        /// <summary>
        /// Defensive position (1-9), or 11/12 for pinch hitter/pinch runner.
        /// </summary>
        public byte FieldingPosition { get; set; }
    }
}
