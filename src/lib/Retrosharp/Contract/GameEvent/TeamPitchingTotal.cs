namespace Retrosharp.Contract.GameEvent
{
    /// <summary>
    /// One team's pitching totals for a single game, aggregated from play-by-play for
    /// reconciliation against <see cref="Game.GamePitchingStatistics"/>. Field names mirror
    /// <see cref="Game.GamePitchingStatistics"/> exactly, except
    /// <see cref="Game.GamePitchingStatistics.TeamEarnedRuns"/>, deliberately not derived --
    /// see spec/game-event.md's Team Earned Runs Future Enhancement note. See
    /// spec/phase-1-build-plan.md Step 6e.
    /// </summary>
    public sealed class TeamPitchingTotal
    {
        public required int FranchiseId { get; init; }

        public byte PitchersUsed { get; init; }

        public short IndividualEarnedRuns { get; init; }

        public byte WildPitches { get; init; }

        public byte Balks { get; init; }
    }
}
