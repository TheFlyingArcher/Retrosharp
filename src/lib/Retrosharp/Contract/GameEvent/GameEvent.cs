using System;

namespace Retrosharp.Contract.GameEvent
{
    /// <summary>
    /// Represents a single play, in chronological order within a game.
    /// See spec/game-event.md, "Data Model" section.
    /// </summary>
    public class GameEvent : Entity
    {
        /// <summary>
        /// Foreign key to the game.
        /// </summary>
        public int GameId { get; set; }

        /// <summary>
        /// Order of this event within the game, used to reconstruct chronology.
        /// </summary>
        public int Sequence { get; set; }

        /// <summary>
        /// Inning number.
        /// </summary>
        public byte Inning { get; set; }

        /// <summary>
        /// Indicates whether the team at bat is home or visitor ("H" or "V").
        /// </summary>
        public string TeamAtBat { get; set; }

        /// <summary>
        /// Foreign key to the batter.
        /// </summary>
        public int BatterId { get; set; }

        /// <summary>
        /// Foreign key to the pitcher, denormalized from the current lineup/substitution
        /// state at the time of the play.
        /// </summary>
        public int PitcherId { get; set; }

        /// <summary>
        /// Count of balls in the plate appearance.
        /// </summary>
        public byte Balls { get; set; }

        /// <summary>
        /// Count of strikes in the plate appearance.
        /// </summary>
        public byte Strikes { get; set; }

        /// <summary>
        /// Count of foul balls hit while the batter already had two strikes.
        /// </summary>
        public byte FoulBallsWithTwoStrikes { get; set; }

        /// <summary>
        /// Raw Retrosheet pitch sequence string.
        /// </summary>
        public string PitchSequence { get; set; }

        /// <summary>
        /// Raw Retrosheet play code string, preserved for traceability back to the source data.
        /// </summary>
        public string RawEventText { get; set; }

        /// <summary>
        /// Categorized event type.
        /// </summary>
        public GameEventType EventType { get; set; }

        /// <summary>
        /// The bundled right-hand event when the play combines two events with "+" in
        /// Retrosheet's raw code (for example "K+SB2", "K+WP", "W+CS2(24)") -- null otherwise.
        /// <see cref="EventType"/> can only hold the left-hand event, so this is what lets
        /// statistics derivation notice a bundled stolen base/caught stealing/wild pitch/balk/
        /// passed ball at all. See spec/phase-1-build-plan.md Step 6e.
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
        public bool IsSacHit { get; set; }

        /// <summary>
        /// Whether the play was a sacrifice fly.
        /// </summary>
        public bool IsSacFly { get; set; }
    }
}
