namespace Retrosharp.Contract.GameEvent
{
    /// <summary>
    /// One team's fielding totals for a single game, aggregated from play-by-play for
    /// reconciliation against <see cref="Game.GameFieldingStatistics"/>. Field names mirror
    /// <see cref="Game.GameFieldingStatistics"/>, except
    /// <see cref="Game.GameFieldingStatistics.PassedBalls"/>/
    /// <see cref="Game.GameFieldingStatistics.DoublePlays"/>/
    /// <see cref="Game.GameFieldingStatistics.TriplePlays"/>, deliberately not derived --
    /// see spec/phase-1-build-plan.md Step 6d's Fielding scope-exclusion note. See
    /// spec/phase-1-build-plan.md Step 6e.
    /// </summary>
    public sealed class TeamFieldingTotal
    {
        public required int FranchiseId { get; init; }

        public short Putouts { get; init; }

        public short Assists { get; init; }

        public short Errors { get; init; }
    }
}
