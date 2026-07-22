namespace Retrosharp.UI.Api.Models
{
    /// <summary>
    /// One batting statistics row (or a combined total across rows, in which case
    /// <see cref="FranchiseCode"/>/<see cref="FranchiseName"/>/<see cref="SeasonYear"/> are null,
    /// since a combined total may span more than one franchise/season).
    /// </summary>
    public class BattingLine
    {
        public string? FranchiseCode { get; set; }

        public string? FranchiseName { get; set; }

        public short? SeasonYear { get; set; }

        public short PlateAppearances { get; set; }

        public short AtBats { get; set; }

        public short Hits { get; set; }

        public short Doubles { get; set; }

        public short Triples { get; set; }

        public short Homeruns { get; set; }

        public short BaseOnBalls { get; set; }

        public short Strikeouts { get; set; }

        public short SacrificeFlies { get; set; }

        public short SacrificeBunts { get; set; }

        public short IntentionalBb { get; set; }

        public short HitByPitches { get; set; }

        public short StolenBases { get; set; }

        public short TimesCaughtStealing { get; set; }

        public short Runs { get; set; }

        public short GroundedIntoDoublePlay { get; set; }

        public int TotalBases { get; set; }

        public float BattingAverage { get; set; }

        public float OnBasePercentage { get; set; }

        public float SluggingPercentage { get; set; }

        public float OnBasePlusSlugging { get; set; }

        public float BattingAverageOnBallsInPlay { get; set; }

        public float IsolatedPower { get; set; }
    }
}
