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
        NoPlay
    }
}
