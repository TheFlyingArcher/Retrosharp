using Mapster;
using Microsoft.AspNetCore.Mvc;
using Retrosharp.Service.Interface;
using Retrosharp.UI.Api.Models;

namespace Retrosharp.UI.Api.Controllers
{
    /// <summary>
    /// Data Viewing endpoints for players (search, identity/biographical detail). Distinct from
    /// PersonController, which only triggers the biofile ETL saga. See spec/api.md.
    /// </summary>
    [ApiController]
    [Route("api/players")]
    public class PlayersController : ControllerBase
    {
        private const int DefaultLimit = 25;
        private const int MaxLimit = 100;

        private readonly IPersonService _personService;

        public PlayersController(IPersonService personService)
        {
            _personService = personService;
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
    }
}
