using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Retrosharp.Data.Model
{
    /// <summary>
    /// Records that a game's statistics have been fully applied to Batting, Pitching, and
    /// Fielding. This is the mechanism used to atomically prevent two concurrent sagas from
    /// double-applying the same shared game (see spec/game-event.md, Considerations and
    /// Data Model sections). Owned entirely by the Game Event Parser — Game remains the
    /// exclusive domain of the Game Log Parser and is never written to by the Game Event Parser.
    /// </summary>
    /// <remarks>
    /// Deliberately does not inherit <see cref="DbModel"/> — GameId is the primary key itself
    /// (rather than a separate identity column) so that only one saga can successfully insert
    /// a status row for a given game, which is the atomic claim mechanism described in
    /// spec/game-event.md.
    /// </remarks>
    [Table("GameEventGameStatus")]
    public class GameEventGameStatusModel
    {
        /// <summary>
        /// Primary key, foreign key to the game.
        /// </summary>
        [Key]
        [ForeignKey("Game")]
        public int GameId { get; set; }

        /// <summary>
        /// Timestamp when this game's statistics were successfully applied.
        /// </summary>
        [Required]
        public DateTime ProcessedUtc { get; set; }

        // Navigation Properties

        /// <summary>
        /// Navigation property for the game.
        /// </summary>
        public GameModel Game { get; set; }
    }
}
