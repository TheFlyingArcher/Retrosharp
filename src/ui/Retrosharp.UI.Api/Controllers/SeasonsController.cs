using Mapster;
using Microsoft.AspNetCore.Mvc;

using Retrosharp.Data;
using Retrosharp.Service.Interface;
using Retrosharp.UI.Api.Models;

namespace Retrosharp.UI.Api.Controllers
{
    /// <summary>
    /// Data Viewing endpoints scoped to a whole season, rather than a single player/team/game.
    /// See spec/api.md.
    /// </summary>
    [ApiController]
    [Route("api/seasons")]
    public class SeasonsController : ControllerBase
    {
        private readonly IStandingsService _standingsService;
        private readonly ITeamStatisticsService _teamStatisticsService;
        private readonly IFranchiseRepository _franchiseRepository;

        public SeasonsController(
            IStandingsService standingsService,
            ITeamStatisticsService teamStatisticsService,
            IFranchiseRepository franchiseRepository)
        {
            _standingsService = standingsService;
            _teamStatisticsService = teamStatisticsService;
            _franchiseRepository = franchiseRepository;
        }

        /// <summary>
        /// Gets every franchise's precomputed standing for one season, ordered by rank. Empty
        /// (not an error) if that season's standings haven't been computed yet -- see
        /// <c>POST /api/standings/compute</c>.
        /// </summary>
        [HttpGet("{year}/standings")]
        public async Task<ActionResult<SeasonStandingsResponse>> GetStandings(short year)
        {
            var standings = (await _standingsService.GetBySeasonAsync(year))
                .OrderBy(s => s.Rank)
                .ToList();

            var entries = new List<SeasonStandingEntry>();
            var franchiseCache = new Dictionary<int, Contract.Franchise.Franchise?>();

            foreach (var standing in standings)
            {
                var franchise = await ResolveFranchiseAsync(standing.FranchiseId, franchiseCache);

                entries.Add(new SeasonStandingEntry
                {
                    FranchiseId = standing.FranchiseId,
                    FranchiseCode = franchise?.FranchiseCode ?? string.Empty,
                    FranchiseName = franchise != null ? $"{franchise.PlayingCity} {franchise.Nickname}" : string.Empty,
                    Standing = standing.Adapt<FranchiseStandingLine>()
                });
            }

            return new SeasonStandingsResponse { SeasonYear = year, Entries = entries };
        }

        /// <summary>
        /// Gets every participating franchise's batting and pitching summary for one season, in
        /// a single response -- backs the Season Detail page's two team-stats tables. See
        /// spec/api.md, "GET /seasons/{year}/teams/stats".
        /// </summary>
        [HttpGet("{year}/teams/stats")]
        public async Task<ActionResult<SeasonTeamStatsResponse>> GetTeamStats(short year)
        {
            var (hitting, pitching) = await _teamStatisticsService.GetSeasonSummariesAsync(year);
            var franchiseCache = new Dictionary<int, Contract.Franchise.Franchise?>();

            var hittingTeams = new List<TeamSeasonBattingSummaryLine>();
            foreach (var summary in hitting)
            {
                var franchise = await ResolveFranchiseAsync(summary.FranchiseId, franchiseCache);

                var battingLine = summary.Batting.Adapt<BattingLine>();
                // TeamBattingStatistics.Hit (inherited from GameBattingStatistics) doesn't
                // name-match BattingLine.Hits -- same fixup TeamsController.GetStats already
                // applies for the single-franchise case.
                battingLine.Hits = summary.Batting.Hit;
                battingLine.SeasonYear = year;

                hittingTeams.Add(new TeamSeasonBattingSummaryLine
                {
                    FranchiseId = summary.FranchiseId,
                    FranchiseCode = franchise?.FranchiseCode ?? string.Empty,
                    FranchiseName = franchise != null ? $"{franchise.PlayingCity} {franchise.Nickname}" : string.Empty,
                    AverageAge = summary.AverageAge,
                    RunsPerGame = summary.RunsPerGame,
                    Batting = battingLine
                });
            }

            var pitchingTeams = new List<TeamSeasonPitchingSummaryLine>();
            foreach (var summary in pitching)
            {
                var franchise = await ResolveFranchiseAsync(summary.FranchiseId, franchiseCache);

                var pitchingLine = summary.Pitching.Adapt<PitchingLine>();
                // PitchingLine.InningsPitchedDisplay has no matching source property to map
                // from -- same fixup GamesController.GetById already applies.
                var outs = summary.Pitching.InningsPitched;
                pitchingLine.InningsPitchedDisplay = $"{outs / 3}.{outs % 3}";
                pitchingLine.SeasonYear = year;

                pitchingTeams.Add(new TeamSeasonPitchingSummaryLine
                {
                    FranchiseId = summary.FranchiseId,
                    FranchiseCode = franchise?.FranchiseCode ?? string.Empty,
                    FranchiseName = franchise != null ? $"{franchise.PlayingCity} {franchise.Nickname}" : string.Empty,
                    AverageAge = summary.AverageAge,
                    RunsAllowedPerGame = summary.RunsAllowedPerGame,
                    Pitching = pitchingLine
                });
            }

            return new SeasonTeamStatsResponse { SeasonYear = year, HittingTeams = hittingTeams, PitchingTeams = pitchingTeams };
        }

        private async Task<Contract.Franchise.Franchise?> ResolveFranchiseAsync(int franchiseId, Dictionary<int, Contract.Franchise.Franchise?> cache)
        {
            if (!cache.TryGetValue(franchiseId, out var franchise))
            {
                franchise = await _franchiseRepository.GetByIdAsync(franchiseId);
                cache[franchiseId] = franchise;
            }

            return franchise;
        }
    }
}
