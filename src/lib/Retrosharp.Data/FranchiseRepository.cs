using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Retrosharp.Contract.Franchise;
using Retrosharp.Data.Context;
using Retrosharp.Data.Model;

namespace Retrosharp.Data
{
    public class FranchiseRepository : BaseRepository<FranchiseModel, Franchise>, IFranchiseRepository
    {
        public FranchiseRepository(RetrosharpContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public async Task<Franchise> GetByFranchiseCodeAsync(string franchiseCode)
        {
            var franchise = await Context.Franchises
                .Where(f => f.FranchiseCode == franchiseCode)
                .ProjectToType<Franchise>()
                .FirstOrDefaultAsync();

            return franchise;
        }

        public async Task<Franchise> GetByFranchiseCodeAndDateAsync(string franchiseCode, DateTime asOfDate)
        {
            var franchise = await Context.Franchises
                .Where(f => f.FranchiseCode == franchiseCode
                    && f.FranchiseStart <= asOfDate
                    && (f.FranchiseEnd == null || f.FranchiseEnd >= asOfDate))
                .ProjectToType<Franchise>()
                .FirstOrDefaultAsync();

            return franchise;
        }

        public async Task<IEnumerable<Franchise>> GetByLeagueIdAsync(int leagueId)
        {
            var franchises = await Context.Franchises
                .Where(f => f.LeagueId == leagueId)
                .ProjectToType<Franchise>()
                .ToListAsync();

            return franchises;
        }

        public async Task<IEnumerable<Franchise>> GetActiveAsync()
        {
            var franchises = await Context.Franchises
                .Where(f => f.IsActive)
                .ProjectToType<Franchise>()
                .ToListAsync();

            return franchises;
        }
    }
}
