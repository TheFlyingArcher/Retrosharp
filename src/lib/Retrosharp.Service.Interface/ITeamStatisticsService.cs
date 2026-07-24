using Retrosharp.Contract.Game;
using Retrosharp.Contract.Pitching;

namespace Retrosharp.Service.Interface
{
    /// <summary>
    /// Service interface for a franchise's team-level season statistics. Unlike player
    /// statistics, there's no "combined total across multiple rows" concept here -- a
    /// team-season is always exactly one aggregate. See spec/api.md, "GET /teams/{id}/stats".
    /// </summary>
    public interface ITeamStatisticsService
    {
        /// <summary>
        /// Team batting stats for one season, summed from <see cref="GameBattingStatistics"/> --
        /// the Game Log Parser's authoritative team-level totals. Null if the franchise has no
        /// games recorded for that season.
        /// </summary>
        Task<TeamBattingStatistics> GetBattingAsync(int franchiseId, short season);

        /// <summary>
        /// Team pitching stats for one season, summed from the franchise's individual pitchers'
        /// <see cref="Pitching"/> rows (GameBattingStatistics's pitching counterpart,
        /// GamePitchingStatistics, doesn't carry hits/walks/strikeouts/innings at team
        /// granularity), except the ERA numerator, which uses the authoritative
        /// <c>GamePitchingStatistics.TeamEarnedRuns</c>. Null if the franchise has no pitching
        /// rows for that season.
        /// </summary>
        Task<PitchingStatistics> GetPitchingAsync(int franchiseId, short season);

        /// <summary>
        /// Team fielding stats for one season, summed from <see cref="GameFieldingStatistics"/>.
        /// Null if the franchise has no games recorded for that season.
        /// </summary>
        Task<TeamFieldingStatistics> GetFieldingAsync(int franchiseId, short season);
    }
}
