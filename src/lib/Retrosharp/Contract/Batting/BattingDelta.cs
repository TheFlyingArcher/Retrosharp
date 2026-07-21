namespace Retrosharp.Contract.Batting
{
    /// <summary>
    /// One player's batting statistical contribution from a single game -- to be added on top
    /// of whatever their season row already holds, not the season totals themselves. Field
    /// names mirror <see cref="Batting"/> exactly (except <see cref="Batting.Positions"/>,
    /// deliberately not derived -- see spec/game-event.md's positions-played Future
    /// Enhancement note). See spec/phase-1-build-plan.md Step 6d.
    /// </summary>
    public sealed class BattingDelta
    {
        public required int PersonId { get; init; }

        public required int FranchiseId { get; init; }

        public required short SeasonYear { get; init; }

        public short PlateAppearances { get; init; }

        public short AtBats { get; init; }

        public short Hits { get; init; }

        public short Doubles { get; init; }

        public short Triples { get; init; }

        public short Homeruns { get; init; }

        public short BaseOnBalls { get; init; }

        public short Strikeouts { get; init; }

        public short SacrificeFlies { get; init; }

        public short SacrificeBunts { get; init; }

        public short IntentionalBb { get; init; }

        public short HitByPitches { get; init; }

        public short StolenBases { get; init; }

        public short TimesCaughtStealing { get; init; }

        public short Runs { get; init; }

        public short GroundedIntoDoublePlay { get; init; }
    }
}
