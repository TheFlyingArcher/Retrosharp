namespace Retrosharp.UI.Api.Models
{
    /// <summary>
    /// One franchise lineage's all-time record. Backs the Franchises page. See spec/api.md,
    /// "GET /teams".
    /// </summary>
    public class FranchiseCareerSummaryLine
    {
        public int FranchiseId { get; set; }

        public string FranchiseCode { get; set; } = string.Empty;

        public string CurrentName { get; set; } = string.Empty;

        public IReadOnlyList<string> FormerNames { get; set; } = Array.Empty<string>();

        public short FirstSeasonYear { get; set; }

        public short GamesPlayed { get; set; }

        public short Wins { get; set; }

        public short Losses { get; set; }

        public short Ties { get; set; }

        public float WinPercentage { get; set; }

        public int SeasonsAboveFiveHundred { get; set; }

        public int SeasonsBelowFiveHundred { get; set; }
    }
}
