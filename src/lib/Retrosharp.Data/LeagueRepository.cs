using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Retrosharp.Contract.League;
using Retrosharp.Data.Context;
using Retrosharp.Data.Model;

namespace Retrosharp.Data
{
    public class LeagueRepository : BaseRepository<LeagueModel, League>, ILeagueRepository
    {
        public LeagueRepository(RetrosharpContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public async Task<League> GetByLeagueCodeAsync(string leagueCode)
        {
            var league = await Context.Leagues
                .Where(l => l.LeagueCode == leagueCode)
                .ProjectToType<League>()
                .FirstOrDefaultAsync();

            return league;
        }
    }
}
