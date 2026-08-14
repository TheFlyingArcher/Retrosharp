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
        private readonly IFranchiseRepository _franchiseRepository;

        public SeasonsController(IStandingsService standingsService, IFranchiseRepository franchiseRepository)
        {
            _standingsService = standingsService;
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
                if (!franchiseCache.TryGetValue(standing.FranchiseId, out var franchise))
                {
                    franchise = await _franchiseRepository.GetByIdAsync(standing.FranchiseId);
                    franchiseCache[standing.FranchiseId] = franchise;
                }

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
    }
}
