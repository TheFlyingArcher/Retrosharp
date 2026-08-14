namespace Retrosharp.Contract.Game
{
    /// <summary>
    /// One franchise's batting summary for one season, as needed for the Season Detail page's
    /// hitters table -- team batting stats plus the average age (as of June 30, see
    /// <see cref="Format.BaseballAge"/>) of every batter who played for them that season, and
    /// runs scored per game. See spec/api.md, "GET /seasons/{year}/teams/stats".
    /// </summary>
    public class TeamSeasonBattingSummary
    {
        public int FranchiseId { get; set; }

        public TeamBattingStatistics Batting { get; set; }

        /// <summary>
        /// Average age (as of June 30 of the season) across every distinct batter who recorded
        /// a Batting row for this franchise-season. 0 if none of them have a known birth date.
        /// </summary>
        public float AverageAge { get; set; }

        public float RunsPerGame { get; set; }
    }
}
