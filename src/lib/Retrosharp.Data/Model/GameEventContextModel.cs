using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Retrosharp.Data.Model
{
    /// <summary>
    /// Per-game metadata parsed from an event file's "info" records (currently just start
    /// time). Owned entirely by the Game Event Parser -- Game remains the exclusive domain of
    /// the Game Log Parser and is never written to by the Game Event Parser. See spec/game-event.md,
    /// "Future Enhancement (Phase 1 gap): Game start time from info records".
    /// </summary>
    /// <remarks>
    /// Deliberately does not inherit <see cref="DbModel"/> -- GameId is the primary key itself,
    /// the same shared-primary-key, at-most-one-row-per-game shape as <see cref="GameEventGameStatusModel"/>.
    /// </remarks>
    [Table("GameEventContext")]
    public class GameEventContextModel
    {
        /// <summary>
        /// Primary key, foreign key to the game.
        /// </summary>
        [Key]
        [ForeignKey("Game")]
        public int GameId { get; set; }

        /// <summary>
        /// The game's local start time, from the event file's "info,starttime,..." record.
        /// </summary>
        public TimeOnly? StartTimeLocal { get; set; }

        // Navigation Properties

        /// <summary>
        /// Navigation property for the game.
        /// </summary>
        public GameModel Game { get; set; }
    }
}
