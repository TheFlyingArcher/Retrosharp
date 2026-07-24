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

        public async Task<(IEnumerable<Franchise> Items, int TotalCount)> SearchAsync(string? q, string? code, short? season, int limit, int offset)
        {
            var query = Context.Franchises.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var searchUpper = q.ToUpper();
                query = query.Where(f =>
                    f.Nickname.ToUpper().Contains(searchUpper) ||
                    f.PlayingCity.ToUpper().Contains(searchUpper) ||
                    (f.AlternateNickname != null && f.AlternateNickname.ToUpper().Contains(searchUpper)));
            }

            if (!string.IsNullOrWhiteSpace(code))
            {
                var codeUpper = code.ToUpper();
                query = query.Where(f => f.FranchiseCode.ToUpper() == codeUpper);
            }

            if (season.HasValue)
            {
                query = query.Where(f => f.FranchiseStart.Year <= season.Value
                    && (f.FranchiseEnd == null || f.FranchiseEnd.Value.Year >= season.Value));
            }

            var totalCount = await query.CountAsync();

            var franchises = await query
                .OrderBy(f => f.PlayingCity)
                .ThenBy(f => f.Nickname)
                .Skip(offset)
                .Take(limit)
                .ProjectToType<Franchise>()
                .ToListAsync();

            return (franchises, totalCount);
        }
    }
}
