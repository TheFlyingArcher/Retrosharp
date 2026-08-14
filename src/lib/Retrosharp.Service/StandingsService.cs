using Retrosharp.Contract.Standing;
using Retrosharp.Data;
using Retrosharp.Format.Standings;
using Retrosharp.Service.Interface;

namespace Retrosharp.Service
{
    public class StandingsService : IStandingsService
    {
        private readonly IGameRepository _gameRepository;
        private readonly IFranchiseRepository _franchiseRepository;
        private readonly IFranchiseSeasonStandingRepository _standingRepository;

        public StandingsService(
            IGameRepository gameRepository,
            IFranchiseRepository franchiseRepository,
            IFranchiseSeasonStandingRepository standingRepository)
        {
            _gameRepository = gameRepository;
            _franchiseRepository = franchiseRepository;
            _standingRepository = standingRepository;
        }

        public async Task<int> RecomputeSeasonAsync(short seasonYear)
        {
            var games = (await _gameRepository.GetBySeasonAsync(seasonYear)).ToList();

            var franchiseIds = games
                .SelectMany(g => new[] { g.HomeFranchiseId, g.VisitorFranchiseId })
                .Distinct();

            var franchiseContext = new Dictionary<int, (int? LeagueId, string? DivisionCode)>();
            foreach (var franchiseId in franchiseIds)
            {
                var franchise = await _franchiseRepository.GetByIdAsync(franchiseId);
                franchiseContext[franchiseId] = (franchise?.LeagueId, franchise?.DivisionCode);
            }

            var standings = StandingsResolver.Resolve(seasonYear, games, franchiseContext);

            await _standingRepository.ReplaceSeasonAsync(seasonYear, standings);

            return standings.Count;
        }

        public Task<FranchiseSeasonStanding> GetByFranchiseSeasonAsync(int franchiseId, short seasonYear) =>
            _standingRepository.GetByFranchiseSeasonAsync(franchiseId, seasonYear);

        public Task<IEnumerable<FranchiseSeasonStanding>> GetBySeasonAsync(short seasonYear) =>
            _standingRepository.GetBySeasonAsync(seasonYear);
    }
}
