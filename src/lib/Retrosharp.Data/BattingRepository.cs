using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Retrosharp.Contract.Batting;
using Retrosharp.Data.Context;
using Retrosharp.Data.Model;

namespace Retrosharp.Data
{
    public class BattingRepository : BaseRepository<BattingModel, Batting>, IBattingRepository
    {
        public BattingRepository(RetrosharpContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public async Task<IEnumerable<Batting>> GetByPersonIdAsync(int personId)
        {
            var batting = await Context.Batting
                .Where(b => b.PersonId == personId)
                .ProjectToType<Batting>()
                .ToListAsync();

            return batting;
        }

        public async Task<Batting> GetByPersonFranchiseSeasonAsync(int personId, int franchiseId, short seasonYear)
        {
            var batting = await Context.Batting
                .Where(b => b.PersonId == personId && 
                           b.FranchiseId == franchiseId && 
                           b.SeasonYear == seasonYear)
                .ProjectToType<Batting>()
                .FirstOrDefaultAsync();

            return batting;
        }
    }
}
