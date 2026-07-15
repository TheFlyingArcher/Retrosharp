using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Retrosharp.Contract.Game;
using Retrosharp.Data.Context;
using Retrosharp.Data.Model;

namespace Retrosharp.Data
{
    public class GameFieldingStatisticsRepository : BaseRepository<GameFieldingStatisticsModel, GameFieldingStatistics>, IGameFieldingStatisticsRepository
    {
        public GameFieldingStatisticsRepository(RetrosharpContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public async Task<IEnumerable<GameFieldingStatistics>> GetByGameIdAsync(int gameId)
        {
            var statistics = await Context.GameFieldingStatistics
                .Where(gs => gs.GameId == gameId)
                .ProjectToType<GameFieldingStatistics>()
                .ToListAsync();

            return statistics;
        }

        public async Task<IEnumerable<GameFieldingStatistics>> GetByFranchiseIdAsync(int franchiseId)
        {
            var statistics = await Context.GameFieldingStatistics
                .Where(gs => gs.FranchiseId == franchiseId)
                .ProjectToType<GameFieldingStatistics>()
                .ToListAsync();

            return statistics;
        }

        public async Task<IEnumerable<GameFieldingStatistics>> GetByFranchiseAndGameAsync(int franchiseId, int gameId)
        {
            var statistics = await Context.GameFieldingStatistics
                .Where(gs => gs.FranchiseId == franchiseId && gs.GameId == gameId)
                .ProjectToType<GameFieldingStatistics>()
                .ToListAsync();

            return statistics;
        }
    }
}
