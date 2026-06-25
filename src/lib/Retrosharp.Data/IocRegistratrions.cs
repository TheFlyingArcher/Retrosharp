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
            services.AddTransient<IGameRepository, GameRepository>();
            services.AddTransient<IGameLineupRepository, GameLineupRepository>();
        }
    }
}
