using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Retrosharp.Contract.Pitching;
using Retrosharp.Data.Context;
using Retrosharp.Data.Model;

namespace Retrosharp.Data
{
    public class PitchingRepository : BaseRepository<PitchingModel, Pitching>, IPitchingRepository
    {
        public PitchingRepository(RetrosharpContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public async Task<IEnumerable<Pitching>> GetByPersonIdAsync(int personId)
        {
            var pitching = await Context.Pitching
                .Where(p => p.PersonId == personId)
                .ProjectToType<Pitching>()
                .ToListAsync();

            return pitching;
        }

        public async Task<Pitching> GetByPersonFranchiseSeasonAsync(int personId, int franchiseId, short seasonYear)
        {
            var pitching = await Context.Pitching
                .Where(p => p.PersonId == personId && 
                           p.FranchiseId == franchiseId && 
                           p.SeasonYear == seasonYear)
                .ProjectToType<Pitching>()
                .FirstOrDefaultAsync();

            return pitching;
        }
    }
}
