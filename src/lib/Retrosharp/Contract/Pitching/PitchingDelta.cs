namespace Retrosharp.Contract.Pitching
{
    /// <summary>
    /// One pitcher's statistical contribution from a single game -- to be added on top of
    /// whatever their season row already holds, not the season totals themselves. Field names
    /// mirror <see cref="Pitching"/> exactly. <see cref="InningsPitched"/> is total outs
    /// recorded, not a fractional inning count (a <c>short</c> can't hold "6.2") -- see
    /// spec/phase-1-build-plan.md Step 6d. <see cref="Saves"/> is always 0 for Phase 1 --
    /// deliberately not derived (see the plan's scope-exclusion note).
    /// </summary>
    public sealed class PitchingDelta
    {
        public required int PersonId { get; init; }

        public required int FranchiseId { get; init; }

        public required short SeasonYear { get; init; }

        public short GamesPitched { get; init; }

        public short GamesStarted { get; init; }

        public short GamesFinished { get; init; }

        public short CompleteGames { get; init; }

        public short Shutouts { get; init; }

        public short Saves { get; init; }

        public short InningsPitched { get; init; }

        public short Hits { get; init; }

        public short Runs { get; init; }

        public short EarnedRuns { get; init; }

        public short BaseOnBalls { get; init; }

        public short Strikeouts { get; init; }

        public short IntentionalBb { get; init; }

        public short HitBatsmen { get; init; }

        public short Balks { get; init; }

        public short WildPitches { get; init; }
    }
}
