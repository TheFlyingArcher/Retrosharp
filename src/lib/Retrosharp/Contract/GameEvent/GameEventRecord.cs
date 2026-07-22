using System.Collections.Generic;

namespace Retrosharp.Contract.GameEvent
{
    /// <summary>
    /// Bundles one game's full resolved play-by-play -- every play plus its runners and
    /// fielding credits, the non-play context records (substitutions, adjustments,
    /// commentary), and the derived Batting/Pitching/Fielding statistical deltas -- so it can
    /// move from the import service to the repository layer as a single unit, mirroring
    /// <see cref="Retrosharp.Contract.Game.GameLogRecord"/>'s existing whole-graph pattern.
    /// <see cref="HomeFranchiseId"/>/<see cref="VisitorFranchiseId"/> let Step 6e's
    /// reconciliation resolver attribute plays to a team the same way
    /// <see cref="Retrosharp.Format.PlayByPlay.GameStatisticsResolver"/> already does. See
    /// spec/phase-1-build-plan.md Steps 6b/6c/6d/6e.
    /// </summary>
    public sealed class GameEventRecord
    {
        public required int GameId { get; init; }

        public required int HomeFranchiseId { get; init; }

        public required int VisitorFranchiseId { get; init; }

        public required IReadOnlyList<GameEventPlayRecord> Plays { get; init; }

        public required IReadOnlyList<GameSubstitution> Substitutions { get; init; }

        public required IReadOnlyList<GameAdjustment> Adjustments { get; init; }

        public required IReadOnlyList<GameComment> Comments { get; init; }

        public required GameStatisticsDelta Statistics { get; init; }
    }
}
