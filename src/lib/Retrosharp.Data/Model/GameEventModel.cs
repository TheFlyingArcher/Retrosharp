using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Retrosharp.Contract.GameEvent;

namespace Retrosharp.Data.Model
{
    /// <summary>
    /// Represents a single play, in chronological order within a game.
    /// See spec/game-event.md, "Data Model" section.
    /// </summary>
    [Table("GameEvent")]
    public class GameEventModel : DbModel
    {
        /// <summary>
        /// Foreign key to the game.
        /// </summary>
        [ForeignKey("Game")]
        [Required]
        public int GameId { get; set; }

        /// <summary>
        /// Order of this event within the game, used to reconstruct chronology.
        /// </summary>
        [Required]
        public int Sequence { get; set; }

        /// <summary>
        /// Inning number.
        /// </summary>
        [Required]
        public byte Inning { get; set; }

        /// <summary>
        /// Indicates whether the team at bat is home or visitor ("H" or "V").
        /// </summary>
        [Required]
        [StringLength(1)]
        public string TeamAtBat { get; set; }

        /// <summary>
        /// Foreign key to the batter.
        /// </summary>
        [ForeignKey("Batter")]
        [Required]
        public int BatterId { get; set; }

        /// <summary>
        /// Foreign key to the pitcher, denormalized from the current lineup/substitution
        /// state at the time of the play.
        /// </summary>
        [ForeignKey("Pitcher")]
        [Required]
        public int PitcherId { get; set; }

        /// <summary>
        /// Count of balls in the plate appearance.
        /// </summary>
        [Required]
        public byte Balls { get; set; }

        /// <summary>
        /// Count of strikes in the plate appearance.
        /// </summary>
        [Required]
        public byte Strikes { get; set; }

        /// <summary>
        /// Count of foul balls hit while the batter already had two strikes.
        /// </summary>
        [Required]
        public byte FoulBallsWithTwoStrikes { get; set; }

        /// <summary>
        /// Raw Retrosheet pitch sequence string.
        /// </summary>
        [StringLength(32)]
        public string PitchSequence { get; set; }

        /// <summary>
        /// Raw Retrosheet play code string, preserved for traceability back to the source data.
        /// </summary>
        [Required]
        [StringLength(128)]
        public string RawEventText { get; set; }

        /// <summary>
        /// Categorized event type.
        /// </summary>
        [Required]
        public GameEventType EventType { get; set; }

        /// <summary>
        /// The bundled right-hand event when the play combines two events with "+" in
        /// Retrosheet's raw code (for example "K+SB2", "K+WP") -- null otherwise. See
        /// spec/phase-1-build-plan.md Step 6e.
        /// </summary>
        public GameEventType? SecondaryEventType { get; set; }

        /// <summary>
        /// Trajectory of a batted ball, tracked independently of <see cref="EventType"/>.
        /// Null when the play did not involve a batted ball in play, such as a walk or strikeout.
        /// </summary>
        public BattedBallType? BattedBallType { get; set; }

        /// <summary>
        /// Whether the play was a sacrifice hit.
        /// </summary>
        [Required]
        public bool IsSacHit { get; set; }

        /// <summary>
        /// Whether the play was a sacrifice fly.
        /// </summary>
        [Required]
        public bool IsSacFly { get; set; }

        // Navigation Properties

        /// <summary>
        /// Navigation property for the game.
        /// </summary>
        public GameModel Game { get; set; }

        /// <summary>
        /// Navigation property for the batter.
        /// </summary>
        public PersonModel Batter { get; set; }

        /// <summary>
        /// Navigation property for the pitcher.
        /// </summary>
        public PersonModel Pitcher { get; set; }

        /// <summary>
        /// Navigation collection for this play's runners. Required for EF Core to fix up
        /// <see cref="GameEventRunnerModel.GameEventId"/> when saving a whole play graph in a
        /// single SaveChangesAsync (see GameEventRepository.BulkInsertAsync).
        /// </summary>
        public ICollection<GameEventRunnerModel> Runners { get; set; } = new List<GameEventRunnerModel>();
    }
}
