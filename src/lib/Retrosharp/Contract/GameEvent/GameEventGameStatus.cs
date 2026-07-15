using System;

namespace Retrosharp.Contract.GameEvent
{
    /// <summary>
    /// Records that a game's statistics have been fully applied to Batting, Pitching, and
    /// Fielding. This is the mechanism used to atomically prevent two concurrent sagas from
    /// double-applying the same shared game (see spec/game-event.md, Considerations and
    /// Data Model sections). Owned entirely by the Game Event Parser — Game remains the
    /// exclusive domain of the Game Log Parser and is never written to by the Game Event Parser.
    /// </summary>
    public class GameEventGameStatus
    {
        /// <summary>
        /// Primary key, foreign key to the game.
        /// </summary>
        public int GameId { get; set; }

        /// <summary>
        /// Timestamp when this game's statistics were successfully applied.
        /// </summary>
        public DateTime ProcessedUtc { get; set; }
    }
}
