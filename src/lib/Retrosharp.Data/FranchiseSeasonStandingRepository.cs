using Mapster;
using Microsoft.EntityFrameworkCore;

using Retrosharp.Contract.Standing;
using Retrosharp.Data.Context;
using Retrosharp.Data.Model;

namespace Retrosharp.Data
{
    public class FranchiseSeasonStandingRepository : IFranchiseSeasonStandingRepository
    {
        private readonly RetrosharpContext _context;

        public FranchiseSeasonStandingRepository(RetrosharpContext context)
        {
            _context = context;
        }

        public async Task<FranchiseSeasonStanding> GetByFranchiseSeasonAsync(int franchiseId, short seasonYear)
        {
            return await _context.Set<FranchiseSeasonStandingModel>()
                .Where(s => s.FranchiseId == franchiseId && s.SeasonYear == seasonYear)
                .ProjectToType<FranchiseSeasonStanding>()
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<FranchiseSeasonStanding>> GetBySeasonAsync(short seasonYear)
        {
            return await _context.Set<FranchiseSeasonStandingModel>()
                .Where(s => s.SeasonYear == seasonYear)
                .OrderBy(s => s.Rank)
                .ProjectToType<FranchiseSeasonStanding>()
                .ToListAsync();
        }

        public async Task<IEnumerable<FranchiseSeasonStanding>> GetAllAsync()
        {
            return await _context.Set<FranchiseSeasonStandingModel>()
                .ProjectToType<FranchiseSeasonStanding>()
                .ToListAsync();
        }

        public async Task ReplaceSeasonAsync(short seasonYear, IEnumerable<FranchiseSeasonStanding> standings)
        {
            await _context.Database.BeginTransactionAsync();

            try
            {
                var existing = await _context.Set<FranchiseSeasonStandingModel>()
                    .Where(s => s.SeasonYear == seasonYear)
                    .ToListAsync();
                _context.Set<FranchiseSeasonStandingModel>().RemoveRange(existing);

                foreach (var standing in standings)
                {
                    _context.Set<FranchiseSeasonStandingModel>().Add(new FranchiseSeasonStandingModel
                    {
                        FranchiseId = standing.FranchiseId,
                        SeasonYear = standing.SeasonYear,
                        Wins = standing.Wins,
                        Losses = standing.Losses,
                        Ties = standing.Ties,
                        Rank = standing.Rank,
                        GamesBehind = standing.GamesBehind,
                        DivisionChampion = standing.DivisionChampion,
                        LeagueBestRecord = standing.LeagueBestRecord
                    });
                }

                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
            }
            catch
            {
                await _context.Database.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
