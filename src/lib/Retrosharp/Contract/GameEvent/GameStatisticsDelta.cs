using System.Collections.Generic;

using Retrosharp.Contract.Batting;
using Retrosharp.Contract.Fielding;
using Retrosharp.Contract.Pitching;

namespace Retrosharp.Contract.GameEvent
{
    /// <summary>
    /// Bundles one game's derived Batting/Pitching/Fielding deltas -- one entry per distinct
    /// player (and, for Fielding, position) touched in that game -- so they can move from the
    /// resolver to the repository layer as a single unit. See spec/phase-1-build-plan.md
    /// Step 6d.
    /// </summary>
    public sealed class GameStatisticsDelta
    {
        public required IReadOnlyList<BattingDelta> Battings { get; init; }

        public required IReadOnlyList<PitchingDelta> Pitchings { get; init; }

        public required IReadOnlyList<FieldingDelta> Fieldings { get; init; }
    }
}
