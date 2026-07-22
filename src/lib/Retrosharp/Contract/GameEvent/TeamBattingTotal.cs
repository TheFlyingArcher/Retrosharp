namespace Retrosharp.Contract.GameEvent
{
    /// <summary>
    /// One team's batting totals for a single game, aggregated from play-by-play for
    /// reconciliation against <see cref="Game.GameBattingStatistics"/>. Field names mirror
    /// <see cref="Game.GameBattingStatistics"/> exactly. See spec/phase-1-build-plan.md
    /// Step 6e.
    /// </summary>
    public sealed class TeamBattingTotal
    {
        public required int FranchiseId { get; init; }

        public short PlateAppearances { get; init; }

        public short AtBats { get; init; }

        public short Hits { get; init; }

        public short Doubles { get; init; }

        public short Triples { get; init; }

        public short Homeruns { get; init; }

        public short RunsBattedIn { get; init; }

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
