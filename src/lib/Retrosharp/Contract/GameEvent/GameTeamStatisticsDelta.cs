using System.Collections.Generic;

namespace Retrosharp.Contract.GameEvent
{
    /// <summary>
    /// Bundles one game's team-level Batting/Pitching/Fielding totals -- one entry per
    /// franchise -- aggregated from play-by-play purely for reconciliation against the
    /// Game Log Parser's <see cref="Game.GameBattingStatistics"/>/
    /// <see cref="Game.GamePitchingStatistics"/>/<see cref="Game.GameFieldingStatistics"/>.
    /// Not persisted anywhere itself. See spec/phase-1-build-plan.md Step 6e.
    /// </summary>
    public sealed class GameTeamStatisticsDelta
    {
        public required IReadOnlyList<TeamBattingTotal> Battings { get; init; }

        public required IReadOnlyList<TeamPitchingTotal> Pitchings { get; init; }

        public required IReadOnlyList<TeamFieldingTotal> Fieldings { get; init; }
    }
}
