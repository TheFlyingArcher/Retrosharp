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

        /// <summary>
        /// True only when this specific runner's own disposition on the play came from an "SB"
        /// sub-code, not merely "some runner present in a play whose overall EventType is
        /// StolenBase" -- a steal's throw going awry can let a *different* runner advance or
        /// score as a side effect without themselves having stolen anything. Transient: used by
        /// <see cref="Format.PlayByPlay.GameStatisticsResolver"/> to derive
        /// Batting.StolenBases correctly; not a persisted column (GameEventRunnerModel has no
        /// matching property).
        /// </summary>
        public bool IsStolenBase { get; set; }

        /// <summary>
        /// True whenever this runner's disposition came from a "CS"/"POCS" sub-code -- set
        /// unconditionally, even when a subsequent throwing error let the runner reach safely
        /// (<see cref="IsOut"/> = false). Official scoring charges a caught stealing when the
        /// runner "is put out, or would have been put out by errorless play," so the attempt is
        /// still charged in that case; only <see cref="IsOut"/> (which must stay accurate for
        /// game-flow/base-state purposes) reflects the error. Transient, same as
        /// <see cref="IsStolenBase"/>: used by <see cref="Format.PlayByPlay.GameStatisticsResolver"/>
        /// to derive Batting.TimesCaughtStealing correctly; not a persisted column
        /// (GameEventRunnerModel has no matching property).
        /// </summary>
        public bool IsCaughtStealingAttempt { get; set; }
    }
}
