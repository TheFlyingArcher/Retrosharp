using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Retrosharp.Contract.Fielding;
using Retrosharp.Data.Context;
using Retrosharp.Data.Model;

namespace Retrosharp.Data
{
    public class FieldingRepository : BaseRepository<FieldingModel, Fielding>, IFieldingRepository
    {
        public FieldingRepository(RetrosharpContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public async Task<IEnumerable<Fielding>> GetByPersonIdAsync(int personId)
        {
            var fielding = await Context.Fielding
                .Where(f => f.PersonId == personId)
                .ProjectToType<Fielding>()
                .ToListAsync();

            return fielding;
        }

        public async Task<Fielding> GetByPersonFranchiseSeasonAsync(int personId, int franchiseId, short seasonYear)
        {
            var fielding = await Context.Fielding
                .Where(f => f.PersonId == personId && 
                           f.FranchiseId == franchiseId && 
                           f.SeasonYear == seasonYear)
                .ProjectToType<Fielding>()
                .FirstOrDefaultAsync();

            return fielding;
        }
    }
}
