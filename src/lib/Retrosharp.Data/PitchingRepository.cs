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

        public async Task<(int BaseOnBalls, int HitBatsmen, int Strikeouts, int InningsPitchedOuts)> GetLeagueTotalsAsync(IEnumerable<int> franchiseIds, short season)
        {
            var franchiseIdList = franchiseIds.ToList();

            var totals = await Context.Pitching
                .Where(p => franchiseIdList.Contains(p.FranchiseId) && p.SeasonYear == season)
                .GroupBy(p => 1)
                .Select(g => new
                {
                    BaseOnBalls = g.Sum(p => (int)p.BaseOnBalls),
                    HitBatsmen = g.Sum(p => (int)p.HitBatsmen),
                    Strikeouts = g.Sum(p => (int)p.Strikeouts),
                    InningsPitchedOuts = g.Sum(p => (int)p.InningsPitched)
                })
                .FirstOrDefaultAsync();

            return totals == null
                ? (0, 0, 0, 0)
                : (totals.BaseOnBalls, totals.HitBatsmen, totals.Strikeouts, totals.InningsPitchedOuts);
        }
    }
}
