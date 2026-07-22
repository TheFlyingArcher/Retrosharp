namespace Retrosharp.UI.Api.Models
{
    /// <summary>
    /// One fielding statistics row for a single position (or a combined total summed across
    /// positions/franchises, in which case <see cref="Position"/> is null).
    /// </summary>
    public class FieldingLine
    {
        public string? FranchiseCode { get; set; }

        public string? FranchiseName { get; set; }

        public short? SeasonYear { get; set; }

        public byte? Position { get; set; }

        public int? Putouts { get; set; }

        public int? Assists { get; set; }

        public int? Errors { get; set; }

        public int? PassedBalls { get; set; }

        public int? DoublePlays { get; set; }

        public int? TriplePlays { get; set; }

        public float FieldingPercentage { get; set; }
    }
}
