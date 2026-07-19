using System;

namespace Retrosharp.Contract.GameEvent
{
    /// <summary>
    /// Categorized outcome of a single play. Represents the primary result of the play,
    /// independent of batted-ball trajectory (see <see cref="BattedBallType"/>).
    /// </summary>
    public enum GameEventType
    {
        Single,
        Double,
        Triple,
        HomeRun,
        Walk,
        IntentionalWalk,
        HitByPitch,
        Strikeout,
        GroundOut,
        FlyOut,
        Error,
        FieldersChoice,
        StolenBase,
        CaughtStealing,
        WildPitch,
        PassedBall,
        Balk,
        Pickoff,
        PickoffCaughtStealing,
        CatcherInterference,
        NoPlay,

        /// <summary>
        /// DI -- the defense makes no attempt to retire a runner taking an extra base.
        /// </summary>
        DefensiveIndifference,

        /// <summary>
        /// OA -- a baserunning advance not otherwise classified by one of the other event types.
        /// </summary>
        OtherAdvance
    }
}
