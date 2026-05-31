using System;
using System.Collections.Generic;
using System.Text;

using Mapster;

using MapsterMapper;

using Microsoft.EntityFrameworkCore;

using Retrosharp.Contract.Game;
using Retrosharp.Data.Context;
using Retrosharp.Data.Model;

namespace Retrosharp.Data
{
    public class GameLineupRepository : BaseRepository<GameLineupModel, GameLineup>, IGameLineupRepository
    {
        public GameLineupRepository(RetrosharpContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public async Task<IEnumerable<GameLineup>> GetByGameIdAsync(int gameId)
        {
            var lineups = await Set
                .Where(g => g.GameId == gameId)
                .ProjectToType<GameLineup>()
                .ToListAsync();

            return lineups;
        }
    }
}
