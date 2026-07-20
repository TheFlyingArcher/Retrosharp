using System.Collections.Generic;

namespace Retrosharp.Contract.GameEvent
{
    /// <summary>
    /// Bundles one game's full resolved play-by-play -- every play plus its runners and
    /// fielding credits, plus the non-play context records (substitutions, adjustments,
    /// commentary) -- so it can move from the import service to the repository layer as a
    /// single unit, mirroring <see cref="Retrosharp.Contract.Game.GameLogRecord"/>'s existing
    /// whole-graph pattern. See spec/phase-1-build-plan.md Steps 6b/6c.
    /// </summary>
    public sealed class GameEventRecord
    {
        public required int GameId { get; init; }

        public required IReadOnlyList<GameEventPlayRecord> Plays { get; init; }

        public required IReadOnlyList<GameSubstitution> Substitutions { get; init; }

        public required IReadOnlyList<GameAdjustment> Adjustments { get; init; }

        public required IReadOnlyList<GameComment> Comments { get; init; }
    }
}
