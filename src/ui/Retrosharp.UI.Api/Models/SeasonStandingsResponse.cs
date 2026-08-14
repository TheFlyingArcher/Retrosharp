namespace Retrosharp.UI.Api.Models
{
    /// <summary>
    /// Every franchise's precomputed standing for one season, ordered by rank. See spec/api.md,
    /// "GET /seasons/{year}/standings".
    /// </summary>
    public class SeasonStandingsResponse
    {
        public short SeasonYear { get; set; }

        public IReadOnlyList<SeasonStandingEntry> Entries { get; set; } = Array.Empty<SeasonStandingEntry>();
    }

    public class SeasonStandingEntry
    {
        public int FranchiseId { get; set; }

        public string FranchiseCode { get; set; } = string.Empty;

        public string FranchiseName { get; set; } = string.Empty;

        public FranchiseStandingLine Standing { get; set; } = new();
    }
}
