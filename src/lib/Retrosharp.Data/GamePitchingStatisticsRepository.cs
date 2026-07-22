using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Retrosharp.Contract.Game;
using Retrosharp.Data.Context;
using Retrosharp.Data.Model;

namespace Retrosharp.Data
{
    public class GamePitchingStatisticsRepository : BaseRepository<GamePitchingStatisticsModel, GamePitchingStatistics>, IGamePitchingStatisticsRepository
    {
        public GamePitchingStatisticsRepository(RetrosharpContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public async Task<IEnumerable<GamePitchingStatistics>> GetByGameIdAsync(int gameId)
        {
            var statistics = await Context.GamePitchingStatistics
                .Where(gs => gs.GameId == gameId)
                .ProjectToType<GamePitchingStatistics>()
                .ToListAsync();

            return statistics;
        }

        public async Task<IEnumerable<GamePitchingStatistics>> GetByFranchiseIdAsync(int franchiseId)
        {
            var statistics = await Context.GamePitchingStatistics
                .Where(gs => gs.FranchiseId == franchiseId)
                .ProjectToType<GamePitchingStatistics>()
                .ToListAsync();

            return statistics;
        }

        public async Task<IEnumerable<GamePitchingStatistics>> GetByFranchiseAndGameAsync(int franchiseId, int gameId)
        {
            var statistics = await Context.GamePitchingStatistics
                .Where(gs => gs.FranchiseId == franchiseId && gs.GameId == gameId)
                .ProjectToType<GamePitchingStatistics>()
                .ToListAsync();

            return statistics;
        }

        public async Task<int> GetLeagueTeamEarnedRunsAsync(IEnumerable<int> franchiseIds, short season)
        {
            var franchiseIdList = franchiseIds.ToList();

            return await Context.GamePitchingStatistics
                .Where(gs => franchiseIdList.Contains(gs.FranchiseId) && gs.Game.GameDate.Year == season)
                .SumAsync(gs => (int)gs.TeamEarnedRuns);
        }
    }
}
