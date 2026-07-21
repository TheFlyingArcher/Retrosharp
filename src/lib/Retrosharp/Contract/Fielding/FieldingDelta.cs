namespace Retrosharp.Contract.Fielding
{
    /// <summary>
    /// One fielder's putout/assist/error contribution from a single game at one position --
    /// to be added on top of whatever their season row already holds for that
    /// (PersonId, FranchiseId, SeasonYear, Position), not the season totals themselves.
    /// <see cref="Fielding.PassedBalls"/>/<see cref="Fielding.DoublePlays"/>/
    /// <see cref="Fielding.TriplePlays"/> are deliberately not derived here -- see
    /// spec/phase-1-build-plan.md Step 6d's scope-exclusion note.
    /// </summary>
    public sealed class FieldingDelta
    {
        public required int PersonId { get; init; }

        public required int FranchiseId { get; init; }

        public required short SeasonYear { get; init; }

        public required byte Position { get; init; }

        public int Putouts { get; init; }

        public int Assists { get; init; }

        public int Errors { get; init; }
    }
}
