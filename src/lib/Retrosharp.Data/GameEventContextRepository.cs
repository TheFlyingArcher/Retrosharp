using MapsterMapper;
using Microsoft.EntityFrameworkCore;

using Retrosharp.Contract.GameEvent;
using Retrosharp.Data.Context;
using Retrosharp.Data.Model;

namespace Retrosharp.Data
{
    public class GameEventContextRepository : IGameEventContextRepository
    {
        private readonly RetrosharpContext _context;
        private readonly IMapper _mapper;

        public GameEventContextRepository(RetrosharpContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<GameEventContext?> GetByGameIdAsync(int gameId)
        {
            var model = await _context.Set<GameEventContextModel>()
                .FirstOrDefaultAsync(c => c.GameId == gameId);

            return model == null ? null : _mapper.Map<GameEventContext>(model);
        }
    }
}
