using System;
using System.Collections.Generic;
using System.Text;

using MapsterMapper;

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
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Game>> GetByVisitorFranchiseIdAsync(int visitorFranchiseId)
        {
            throw new NotImplementedException();
        }
    }
}
