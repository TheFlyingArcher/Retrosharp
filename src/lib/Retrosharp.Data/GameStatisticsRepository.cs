using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Retrosharp.Contract.Game;
using Retrosharp.Data.Context;
using Retrosharp.Data.Model;

namespace Retrosharp.Data
{
    public class GameStatisticsRepository : BaseRepository<GameStatisticsModel, GameStatistics>, IGameStatisticsRepository
    {
        public GameStatisticsRepository(RetrosharpContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public async Task<IEnumerable<GameStatistics>> GetByGameIdAsync(int gameId)
        {
            var statistics = await Context.GameStatistics
                .Where(gs => gs.GameId == gameId)
                .ProjectToType<GameStatistics>()
                .ToListAsync();

            return statistics;
        }

        public async Task<IEnumerable<GameStatistics>> GetByFranchiseIdAsync(int franchiseId)
        {
            var statistics = await Context.GameStatistics
                .Where(gs => gs.FranchiseId == franchiseId)
                .ProjectToType<GameStatistics>()
                .ToListAsync();

            return statistics;
        }

        public async Task<IEnumerable<GameStatistics>> GetByFranchiseAndGameAsync(int franchiseId, int gameId)
        {
            var statistics = await Context.GameStatistics
                .Where(gs => gs.FranchiseId == franchiseId && gs.GameId == gameId)
                .ProjectToType<GameStatistics>()
                .ToListAsync();

            return statistics;
        }
    }
}
