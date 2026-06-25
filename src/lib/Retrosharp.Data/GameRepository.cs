using System;
using System.Collections.Generic;
using System.Text;

using MapsterMapper;
using Microsoft.EntityFrameworkCore;

using Retrosharp.Contract.Game;
using Retrosharp.Data.Context;
using Retrosharp.Data.Model;

namespace Retrosharp.Data
{
    public class GameRepository : BaseRepository<GameModel, Game>, IGameRepository
    {
        public GameRepository(RetrosharpContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public async Task<IEnumerable<Game>> GetByHomeFranchiseIdAsync(int homeFranchiseId)
        {
            var models = await Context.Set<GameModel>()
                .Where(g => g.HomeFranchiseId == homeFranchiseId)
                .ToListAsync();

            return models.Select(m => Mapper.Map<Game>(m));
        }

        public async Task<IEnumerable<Game>> GetByVisitorFranchiseIdAsync(int visitorFranchiseId)
        {
            var models = await Context.Set<GameModel>()
                .Where(g => g.VisitorFranchiseId == visitorFranchiseId)
                .ToListAsync();

            return models.Select(m => Mapper.Map<Game>(m));
        }
    }
}
