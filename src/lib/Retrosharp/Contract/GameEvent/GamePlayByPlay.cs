using System;
using System.Collections.Generic;

namespace Retrosharp.Contract.GameEvent
{
    /// <summary>
    /// One game's complete play-by-play, reconstructed from persisted <see cref="GameEvent"/>/
    /// <see cref="GameEventRunner"/> rows for reuse by the same resolvers Step 6d/6e already
    /// built (<see cref="Format.PlayByPlay.GameStatisticsResolver"/>,
    /// <see cref="Format.PlayByPlay.GameReconciliationResolver"/>). See spec/api.md,
    /// "Player game logs are derived on demand, not stored".
    /// </summary>
    public sealed class GamePlayByPlay
    {
        public required int HomeFranchiseId { get; init; }

        public required int VisitorFranchiseId { get; init; }

        public required DateTime GameDate { get; init; }

        public required IReadOnlyList<GameEventPlayRecord> Plays { get; init; }
    }
}
