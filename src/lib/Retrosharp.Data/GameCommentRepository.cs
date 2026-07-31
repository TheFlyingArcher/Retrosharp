using Mapster;

using MapsterMapper;

using Microsoft.EntityFrameworkCore;

using Retrosharp.Contract.GameEvent;
using Retrosharp.Data.Context;
using Retrosharp.Data.Model;

namespace Retrosharp.Data
{
    public class GameCommentRepository : BaseRepository<GameCommentModel, GameComment>, IGameCommentRepository
    {
        public GameCommentRepository(RetrosharpContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public async Task<IEnumerable<GameComment>> GetByGameIdAsync(int gameId)
        {
            return await Set
                .Where(c => c.GameId == gameId)
                .OrderBy(c => c.RecordIndex)
                .ProjectToType<GameComment>()
                .ToListAsync();
        }
    }
}
