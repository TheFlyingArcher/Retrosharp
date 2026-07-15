using System;

namespace Retrosharp.Contract.GameEvent
{
    /// <summary>
    /// Trajectory of a batted ball, tracked independently of <see cref="GameEventType"/>
    /// so that statistics like HR/FB (home runs per fly ball allowed) can be computed
    /// regardless of whether a given batted ball was caught, dropped, or hit for extra bases.
    /// </summary>
    public enum BattedBallType
    {
        GroundBall,
        LineDrive,
        FlyBall,
        PopUp
    }
}
