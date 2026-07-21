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
            // Entity repositories
            services.AddTransient<IPersonRepository, PersonRepository>();
            services.AddTransient<IFranchiseRepository, FranchiseRepository>();
            services.AddTransient<ILeagueRepository, LeagueRepository>();
            services.AddTransient<IBallparkRepository, BallparkRepository>();
            services.AddTransient<IGameRepository, GameRepository>();
            services.AddTransient<IGameLineupRepository, GameLineupRepository>();
            services.AddTransient<IGameEventRepository, GameEventRepository>();
            services.AddTransient<IGameStatisticsRepository, GameStatisticsRepository>();
            services.AddTransient<IGameBattingStatisticsRepository, GameBattingStatisticsRepository>();
            services.AddTransient<IGamePitchingStatisticsRepository, GamePitchingStatisticsRepository>();
            services.AddTransient<IGameFieldingStatisticsRepository, GameFieldingStatisticsRepository>();

            // Statistics repositories
            services.AddTransient<IBattingRepository, BattingRepository>();
            services.AddTransient<IPitchingRepository, PitchingRepository>();
            services.AddTransient<IFieldingRepository, FieldingRepository>();
        }
    }
}
