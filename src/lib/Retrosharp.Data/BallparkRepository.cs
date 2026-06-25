using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Retrosharp.Contract.Ballpark;
using Retrosharp.Data.Context;
using Retrosharp.Data.Model;

namespace Retrosharp.Data
{
    public class BallparkRepository : BaseRepository<BallparkModel, Ballpark>, IBallparkRepository
    {
        public BallparkRepository(RetrosharpContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public async Task<Ballpark> GetBySiteCodeAsync(string siteCode)
        {
            var ballpark = await Context.Ballparks
                .Where(b => b.SiteCode == siteCode)
                .ProjectToType<Ballpark>()
                .FirstOrDefaultAsync();

            return ballpark;
        }
    }
}
