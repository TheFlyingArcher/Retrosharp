using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Retrosharp.Contract.Game;
using Retrosharp.Data.Context;
using Retrosharp.Data.Model;

namespace Retrosharp.Data
{
    public class GameBattingStatisticsRepository : BaseRepository<GameBattingStatisticsModel, GameBattingStatistics>, IGameBattingStatisticsRepository
    {
        public GameBattingStatisticsRepository(RetrosharpContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public async Task<IEnumerable<GameBattingStatistics>> GetByGameIdAsync(int gameId)
        {
            var statistics = await Context.GameBattingStatistics
                .Where(gs => gs.GameId == gameId)
                .ProjectToType<GameBattingStatistics>()
                .ToListAsync();

            return statistics;
        }

        public async Task<IEnumerable<GameBattingStatistics>> GetByFranchiseIdAsync(int franchiseId)
        {
            var statistics = await Context.GameBattingStatistics
                .Where(gs => gs.FranchiseId == franchiseId)
                .ProjectToType<GameBattingStatistics>()
                .ToListAsync();

            return statistics;
        }

        public async Task<IEnumerable<GameBattingStatistics>> GetByFranchiseAndGameAsync(int franchiseId, int gameId)
        {
            var statistics = await Context.GameBattingStatistics
                .Where(gs => gs.FranchiseId == franchiseId && gs.GameId == gameId)
                .ProjectToType<GameBattingStatistics>()
                .ToListAsync();

            return statistics;
        }
    }
}
