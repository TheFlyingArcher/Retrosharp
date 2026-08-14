using Microsoft.AspNetCore.Mvc;

using Retrosharp.Service.Interface;
using Retrosharp.UI.Api.Models;

namespace Retrosharp.UI.Api.Controllers
{
    /// <summary>
    /// Triggers (re)computation of precomputed franchise-season standings. Distinct from the
    /// Person/GameLog/GameEvent import endpoints -- this has no external file to parse and
    /// completes synchronously, so it doesn't need saga/service-bus infrastructure, just an
    /// idempotent recompute over already-imported <c>Game</c> rows. See spec/api.md.
    /// </summary>
    [ApiController]
    [Route("api/standings")]
    public class StandingsController : ControllerBase
    {
        private readonly IStandingsService _standingsService;

        public StandingsController(IStandingsService standingsService)
        {
            _standingsService = standingsService;
        }

        /// <summary>
        /// Recomputes standings for <paramref name="season"/> from that season's currently-
        /// imported Game rows. Safe to re-run at any time (for example, after importing more of
        /// that season's Game Log data) -- always replaces the season's rows wholesale.
        /// </summary>
        [HttpPost("compute")]
        public async Task<ActionResult<StandingsComputeResult>> Compute([FromQuery] short season)
        {
            var franchiseCount = await _standingsService.RecomputeSeasonAsync(season);

            return new StandingsComputeResult { SeasonYear = season, FranchiseCount = franchiseCount };
        }
    }
}
