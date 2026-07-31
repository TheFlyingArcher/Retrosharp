using Mapster;

using MapsterMapper;

using Microsoft.EntityFrameworkCore;

using Retrosharp.Contract.GameEvent;
using Retrosharp.Data.Context;
using Retrosharp.Data.Model;

namespace Retrosharp.Data
{
    public class GameAdjustmentRepository : BaseRepository<GameAdjustmentModel, GameAdjustment>, IGameAdjustmentRepository
    {
        public GameAdjustmentRepository(RetrosharpContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public async Task<IEnumerable<GameAdjustment>> GetByGameIdAsync(int gameId)
        {
            return await Set
                .Where(a => a.GameId == gameId)
                .OrderBy(a => a.RecordIndex)
                .ProjectToType<GameAdjustment>()
                .ToListAsync();
        }
    }
}
