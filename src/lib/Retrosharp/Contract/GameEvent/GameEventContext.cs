using System;

namespace Retrosharp.Contract.GameEvent
{
    /// <summary>
    /// Per-game metadata parsed from an event file's "info" records that doesn't belong on
    /// <c>GameEventGameStatus</c> (a narrow concurrency-control marker, not a general metadata
    /// store) or on <c>Game</c> (the Game Log Parser's exclusive domain -- never written to by
    /// the Game Event Parser). At most one row per game, present only when the source event
    /// file had a value to record. See spec/game-event.md, "Future Enhancement (Phase 1 gap):
    /// Game start time from info records".
    /// </summary>
    public class GameEventContext
    {
        /// <summary>
        /// Primary key, foreign key to the game.
        /// </summary>
        public int GameId { get; set; }

        /// <summary>
        /// The game's local start time, from the event file's "info,starttime,..." record. Null
        /// for a game whose event file had no parseable starttime -- distinct from there being
        /// no <see cref="GameEventContext"/> row at all (no event file imported for this game).
        /// </summary>
        public TimeOnly? StartTimeLocal { get; set; }
    }
}
