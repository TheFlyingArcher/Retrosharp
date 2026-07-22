using Mapster;
using Microsoft.AspNetCore.Mvc;
using Retrosharp.Contract;
using Retrosharp.Service.Interface;
using Retrosharp.UI.Api.Models;

namespace Retrosharp.UI.Api.Controllers
{
    /// <summary>
    /// Data Viewing endpoints for players (search, identity/biographical detail, statistics).
    /// Distinct from PersonController, which only triggers the biofile ETL saga. See spec/api.md.
    /// </summary>
    [ApiController]
    [Route("api/players")]
    public class PlayersController : ControllerBase
    {
        private const int DefaultLimit = 25;
        private const int MaxLimit = 100;

        private readonly IPersonService _personService;
        private readonly IPlayerStatisticsService _playerStatisticsService;

        public PlayersController(IPersonService personService, IPlayerStatisticsService playerStatisticsService)
        {
            _personService = personService;
            _playerStatisticsService = playerStatisticsService;
        }

        /// <summary>
        /// Searches players by name (surname, use name, or full name).
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<PagedResult<PlayerSearchResult>>> Search(
            [FromQuery] string q,
            [FromQuery] int limit = DefaultLimit,
            [FromQuery] int offset = 0)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest("q is required.");

            if (limit <= 0 || limit > MaxLimit)
                return BadRequest($"limit must be between 1 and {MaxLimit}.");

            if (offset < 0)
                return BadRequest("offset must be non-negative.");

            var (people, totalCount) = await _personService.SearchByNameAsync(q, limit, offset);

            return new PagedResult<PlayerSearchResult>
            {
                Items = people.Adapt<IEnumerable<PlayerSearchResult>>(),
                TotalCount = totalCount,
                Limit = limit,
                Offset = offset
            };
        }

        /// <summary>
        /// Gets a player's identity/biographical detail.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<PlayerDetail>> GetById(int id)
        {
            var person = await _personService.GetByIdAsync(id);
            if (person == null)
                return NotFound();

            return person.Adapt<PlayerDetail>();
        }

        /// <summary>
        /// Gets a player's batting statistics for one season, or their whole career.
        /// </summary>
        [HttpGet("{id}/batting")]
        public async Task<ActionResult<PlayerStatsResponse<BattingLine>>> GetBatting(int id, [FromQuery] short? season)
        {
            if (await _personService.GetByIdAsync(id) == null)
                return NotFound();

            var stats = await _playerStatisticsService.GetBattingAsync(id, season);

            var rows = stats.Rows.Select(row =>
            {
                var line = row.Stats.Adapt<BattingLine>();
                line.FranchiseCode = row.FranchiseCode;
                line.FranchiseName = row.FranchiseName;
                return line;
            });

            BattingLine? combinedTotal = null;
            if (stats.CombinedTotal != null)
            {
                combinedTotal = stats.CombinedTotal.Adapt<BattingLine>();
                combinedTotal.SeasonYear = null;
            }

            return new PlayerStatsResponse<BattingLine> { Rows = rows, CombinedTotal = combinedTotal };
        }

        /// <summary>
        /// Gets a player's pitching statistics for one season, or their whole career.
        /// </summary>
        [HttpGet("{id}/pitching")]
        public async Task<ActionResult<PlayerStatsResponse<PitchingLine>>> GetPitching(int id, [FromQuery] short? season)
        {
            if (await _personService.GetByIdAsync(id) == null)
                return NotFound();

            var stats = await _playerStatisticsService.GetPitchingAsync(id, season);

            var rows = stats.Rows.Select(row =>
            {
                var line = row.Stats.Adapt<PitchingLine>();
                line.FranchiseCode = row.FranchiseCode;
                line.FranchiseName = row.FranchiseName;
                return line;
            });

            PitchingLine? combinedTotal = null;
            if (stats.CombinedTotal != null)
            {
                combinedTotal = stats.CombinedTotal.Adapt<PitchingLine>();
                combinedTotal.SeasonYear = null;
            }

            return new PlayerStatsResponse<PitchingLine> { Rows = rows, CombinedTotal = combinedTotal };
        }

        /// <summary>
        /// Gets a player's fielding statistics for one season, or their whole career, broken out
        /// by position.
        /// </summary>
        [HttpGet("{id}/fielding")]
        public async Task<ActionResult<PlayerStatsResponse<FieldingLine>>> GetFielding(int id, [FromQuery] short? season)
        {
            if (await _personService.GetByIdAsync(id) == null)
                return NotFound();

            var stats = await _playerStatisticsService.GetFieldingAsync(id, season);

            var rows = stats.Rows.Select(row =>
            {
                var line = row.Stats.Adapt<FieldingLine>();
                line.FranchiseCode = row.FranchiseCode;
                line.FranchiseName = row.FranchiseName;
                return line;
            });

            FieldingLine? combinedTotal = null;
            if (stats.CombinedTotal != null)
            {
                combinedTotal = stats.CombinedTotal.Adapt<FieldingLine>();
                combinedTotal.SeasonYear = null;
                combinedTotal.Position = null;
            }

            return new PlayerStatsResponse<FieldingLine> { Rows = rows, CombinedTotal = combinedTotal };
        }
    }
}
