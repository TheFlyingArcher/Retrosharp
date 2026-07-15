using System;

namespace Retrosharp.Contract.GameEvent
{
    /// <summary>
    /// Represents a person affected as a baserunner by a play, including the batter — a batter
    /// reaching base is simply a runner whose <see cref="StartBase"/> is the batter's box.
    /// See spec/game-event.md, "Data Model" section.
    /// </summary>
    public class GameEventRunner : Entity
    {
        /// <summary>
        /// Foreign key to the game event (play).
        /// </summary>
        public int GameEventId { get; set; }

        /// <summary>
        /// Foreign key to the person (the runner, or the batter acting as a runner).
        /// </summary>
        public int PersonId { get; set; }

        /// <summary>
        /// Base the runner started the play on.
        /// </summary>
        public BaseState StartBase { get; set; }

        /// <summary>
        /// Base the runner was attempting to reach, or reached, as a result of the play.
        /// </summary>
        public BaseState EndBase { get; set; }

        /// <summary>
        /// Whether the runner was put out attempting to reach <see cref="EndBase"/>.
        /// </summary>
        public bool IsOut { get; set; }

        /// <summary>
        /// Whether this runner's advance to home is credited as an RBI to the batter.
        /// Sourced directly from Retrosheet's own (RBI)/(NORBI)/(NR) play-code annotations
        /// rather than independently derived by re-applying official scoring rules.
        /// </summary>
        public bool IsRBI { get; set; }

        /// <summary>
        /// Whether this runner's run, if scored, is earned.
        /// </summary>
        public bool IsEarnedRun { get; set; }

        /// <summary>
        /// Foreign key to the pitcher charged with this runner if they score
        /// (accounts for inherited runners). Null unless the runner scores.
        /// </summary>
        public int? ResponsiblePitcherId { get; set; }
    }
}
