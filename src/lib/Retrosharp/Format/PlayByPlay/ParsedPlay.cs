using System.Collections.Generic;

using Retrosharp.Contract.GameEvent;

namespace Retrosharp.Format.PlayByPlay
{
    /// <summary>
    /// The structured result of parsing one Retrosheet play-code string (for example,
    /// "S9/G34" or "64(1)3/GDP"). Carries no resolved <c>PersonId</c>s -- only the raw
    /// shape of the play (base movement, fielder numbers) -- since resolving those against
    /// lineup/substitution state happens downstream. See spec/game-event.md, "Data Model".
    /// </summary>
    public sealed class ParsedPlay
    {
        public required GameEventType EventType { get; init; }

        /// <summary>
        /// Null when the play did not involve a batted ball in play, such as a walk or
        /// strikeout.
        /// </summary>
        public BattedBallType? BattedBallType { get; init; }

        public bool IsSacHit { get; init; }

        public bool IsSacFly { get; init; }

        public byte Balls { get; init; }

        public byte Strikes { get; init; }

        public byte FoulBallsWithTwoStrikes { get; init; }

        public required string RawEventText { get; init; }

        public required IReadOnlyList<ParsedRunnerAdvance> Runners { get; init; }
    }
}
