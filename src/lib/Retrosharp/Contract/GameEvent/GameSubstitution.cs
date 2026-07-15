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
        /// Order of this substitution within the game.
        /// </summary>
        public int Sequence { get; set; }

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
