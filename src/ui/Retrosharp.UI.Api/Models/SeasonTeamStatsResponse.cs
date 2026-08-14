namespace Retrosharp.UI.Api.Models
{
    /// <summary>
    /// Every participating franchise's batting and pitching summary for one season, in a single
    /// response -- backs the Season Detail page's two team-stats tables. See spec/api.md,
    /// "GET /seasons/{year}/teams/stats".
    /// </summary>
    public class SeasonTeamStatsResponse
    {
        public short SeasonYear { get; set; }

        public IReadOnlyList<TeamSeasonBattingSummaryLine> HittingTeams { get; set; } = Array.Empty<TeamSeasonBattingSummaryLine>();

        public IReadOnlyList<TeamSeasonPitchingSummaryLine> PitchingTeams { get; set; } = Array.Empty<TeamSeasonPitchingSummaryLine>();
    }

    public class TeamSeasonBattingSummaryLine
    {
        public int FranchiseId { get; set; }

        public string FranchiseCode { get; set; } = string.Empty;

        public string FranchiseName { get; set; } = string.Empty;

        /// <summary>Age as of June 30 of the season, averaged across every batter who played for this team.</summary>
        public float AverageAge { get; set; }

        public float RunsPerGame { get; set; }

        public BattingLine Batting { get; set; } = new();
    }

    public class TeamSeasonPitchingSummaryLine
    {
        public int FranchiseId { get; set; }

        public string FranchiseCode { get; set; } = string.Empty;

        public string FranchiseName { get; set; } = string.Empty;

        /// <summary>Age as of June 30 of the season, averaged across every pitcher who pitched for this team.</summary>
        public float AverageAge { get; set; }

        public float RunsAllowedPerGame { get; set; }

        public PitchingLine Pitching { get; set; } = new();
    }
}
