using Mapster;

using MapsterMapper;

using Microsoft.EntityFrameworkCore;

using Retrosharp.Contract.GameEvent;
using Retrosharp.Data.Context;
using Retrosharp.Data.Model;

namespace Retrosharp.Data
{
    public class GameSubstitutionRepository : BaseRepository<GameSubstitutionModel, GameSubstitution>, IGameSubstitutionRepository
    {
        public GameSubstitutionRepository(RetrosharpContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public async Task<IEnumerable<GameSubstitution>> GetByGameIdAsync(int gameId)
        {
            return await Set
                .Where(s => s.GameId == gameId)
                .OrderBy(s => s.RecordIndex)
                .ProjectToType<GameSubstitution>()
                .ToListAsync();
        }
    }
}
