using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Retrosharp.Data.Context;
using Retrosharp.DI;
namespace Retrosharp.Data
{
    public class IocRegistratrions : IRegister
    {
        public async Task Register(IServiceCollection services)
        {
            services.AddDbContext<RetrosharpContext>(b => b.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Retrosharp;Trusted_Connection=true;"));

            services.AddTransient<IGameRepository, GameRepository>();
            services.AddTransient<IGameLineupRepository, GameLineupRepository>();
        }
    }
}
