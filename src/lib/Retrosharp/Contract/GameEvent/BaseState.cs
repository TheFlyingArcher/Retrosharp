using System;

namespace Retrosharp.Contract.GameEvent
{
    /// <summary>
    /// A base a runner started at or was attempting to reach on a play. Used for both
    /// <see cref="GameEventRunner.StartBase"/> and <see cref="GameEventRunner.EndBase"/> —
    /// a runner's <c>EndBase</c> is the base they were attempting to reach, and
    /// <c>IsOut</c> distinguishes whether they reached it safely or were retired there.
    /// </summary>
    public enum BaseState
    {
        /// <summary>
        /// The batter's box. Only valid as a <c>StartBase</c> — a batter becoming a runner
        /// starts here, not on one of the bases.
        /// </summary>
        BattersBox = 0,
        First = 1,
        Second = 2,
        Third = 3,
        Home = 4
    }
}
