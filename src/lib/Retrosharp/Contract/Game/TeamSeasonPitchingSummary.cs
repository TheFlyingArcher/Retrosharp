using Retrosharp.Contract.Pitching;

namespace Retrosharp.Contract.Game
{
    /// <summary>
    /// One franchise's pitching summary for one season, as needed for the Season Detail page's
    /// pitchers table -- team pitching stats plus the average age (as of June 30, see
    /// <see cref="Format.BaseballAge"/>) of every pitcher who pitched for them that season, and
    /// runs allowed per game. See spec/api.md, "GET /seasons/{year}/teams/stats".
    /// </summary>
    public class TeamSeasonPitchingSummary
    {
        public int FranchiseId { get; set; }

        public PitchingStatistics Pitching { get; set; }

        /// <summary>
        /// Average age (as of June 30 of the season) across every distinct pitcher who recorded
        /// a Pitching row for this franchise-season. 0 if none of them have a known birth date.
        /// </summary>
        public float AverageAge { get; set; }

        public float RunsAllowedPerGame { get; set; }
    }
}
