using Mapster;
using Microsoft.AspNetCore.Mvc;
using Retrosharp.Service.Interface;
using Retrosharp.UI.Api.Models;

namespace Retrosharp.UI.Api.Controllers
{
    /// <summary>
    /// Data Viewing endpoints for teams (franchises): search, identity detail, roster, and
    /// season statistics. See spec/api.md.
    /// </summary>
    [ApiController]
    [Route("api/teams")]
    public class TeamsController : ControllerBase
    {
        private const int DefaultLimit = 25;
        private const int MaxLimit = 100;

        private readonly ITeamService _teamService;
        private readonly ITeamStatisticsService _teamStatisticsService;

        public TeamsController(ITeamService teamService, ITeamStatisticsService teamStatisticsService)
        {
            _teamService = teamService;
            _teamStatisticsService = teamStatisticsService;
        }

        /// <summary>
        /// Searches teams by name/nickname/city and/or franchise code. Unlike player search,
        /// neither <paramref name="q"/> nor <paramref name="code"/> is required.
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<PagedResult<TeamSearchResult>>> Search(
            [FromQuery] string? q,
            [FromQuery] string? code,
            [FromQuery] short? season,
            [FromQuery] int limit = DefaultLimit,
            [FromQuery] int offset = 0)
        {
            if (limit <= 0 || limit > MaxLimit)
                return BadRequest($"limit must be between 1 and {MaxLimit}.");

            if (offset < 0)
                return BadRequest("offset must be non-negative.");

            var (teams, totalCount) = await _teamService.SearchAsync(q, code, season, limit, offset);

            return new PagedResult<TeamSearchResult>
            {
                Items = teams.Adapt<IEnumerable<TeamSearchResult>>(),
                TotalCount = totalCount,
                Limit = limit,
                Offset = offset
            };
        }

        /// <summary>
        /// Gets a franchise's identity detail.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<TeamDetail>> GetById(int id)
        {
            var franchise = await _teamService.GetByIdAsync(id);
            if (franchise == null)
                return NotFound();

            return franchise.Adapt<TeamDetail>();
        }

        /// <summary>
        /// Gets every player who recorded a batting, pitching, or fielding row for this
        /// franchise, for one season or across its whole history.
        /// </summary>
        [HttpGet("{id}/roster")]
        public async Task<ActionResult<IEnumerable<PlayerSearchResult>>> GetRoster(int id, [FromQuery] short? season)
        {
            if (await _teamService.GetByIdAsync(id) == null)
                return NotFound();

            var roster = await _teamService.GetRosterAsync(id, season);
            return Ok(roster.Adapt<IEnumerable<PlayerSearchResult>>());
        }

        /// <summary>
        /// Gets a franchise's season batting/pitching/fielding statistics.
        /// </summary>
        [HttpGet("{id}/stats")]
        public async Task<ActionResult<TeamSeasonStatsResponse>> GetStats(int id, [FromQuery] short? season)
        {
            if (season == null)
                return BadRequest("season is required.");

            if (await _teamService.GetByIdAsync(id) == null)
                return NotFound();

            var battingStats = await _teamStatisticsService.GetBattingAsync(id, season.Value);
            var pitchingStats = await _teamStatisticsService.GetPitchingAsync(id, season.Value);
            var fieldingStats = await _teamStatisticsService.GetFieldingAsync(id, season.Value);

            var battingLine = battingStats?.Adapt<BattingLine>();
            if (battingLine != null)
            {
                battingLine.SeasonYear = season;
                // TeamBattingStatistics.Hit (inherited from GameBattingStatistics, correctly
                // named there) doesn't name-match BattingLine.Hits -- unlike the player-level
                // Batting.Hits this DTO was built for, so it needs an explicit fixup here.
                battingLine.Hits = battingStats!.Hit;
            }

            var pitchingLine = pitchingStats?.Adapt<PitchingLine>();
            if (pitchingLine != null)
                pitchingLine.SeasonYear = season;

            var fieldingLine = fieldingStats?.Adapt<FieldingLine>();
            if (fieldingLine != null)
                fieldingLine.SeasonYear = season;

            return new TeamSeasonStatsResponse
            {
                Batting = battingLine,
                Pitching = pitchingLine,
                Fielding = fieldingLine
            };
        }

        /// <summary>
        /// Gets the manager(s) who ran this franchise during the given season, chronologically
        /// ordered. A season with a mid-season managerial change returns more than one entry.
        /// </summary>
        [HttpGet("{id}/managers")]
        public async Task<ActionResult<IEnumerable<TeamManagerHistoryEntry>>> GetManagerHistory(int id, [FromQuery] short? season)
        {
            if (season == null)
                return BadRequest("season is required.");

            if (await _teamService.GetByIdAsync(id) == null)
                return NotFound();

            var history = await _teamService.GetManagerHistoryAsync(id, season.Value);

            return Ok(history.Adapt<IEnumerable<TeamManagerHistoryEntry>>());
        }
    }
}
