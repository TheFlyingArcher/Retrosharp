using System;
using System.Collections.Generic;
using System.Text;

using Mapster;

using Microsoft.Extensions.DependencyInjection;

using DI = Retrosharp.DI;

using Retrosharp.Format;
using Retrosharp.Service.ETL;
using Retrosharp.Service.Interface;
using Retrosharp.Service.Interface.ETL;

namespace Retrosharp.Service
{
    public class IocRegistrations : DI.IRegister
    {
        public async Task Register(IServiceCollection services)
        {
            // ETL file services
            services.AddTransient<IRetrosheetFileService<GameLog>, GameLogFileService>();
            services.AddTransient<IRetrosheetFileService<BioFile>, BioFileService>();

            // Business logic services
            services.AddTransient<IPersonService, PersonService>();
            services.AddTransient<IPersonImportService, PersonImportService>();
            services.AddTransient<IGameLogImportService, GameLogImportService>();
            services.AddTransient<IGameEventImportService, GameEventImportService>();
            services.AddTransient<IGameService, GameService>();
            services.AddTransient<IBattingService, BattingService>();
            services.AddTransient<IPlayerStatisticsService, PlayerStatisticsService>();
            services.AddTransient<IPlayerGameLogService, PlayerGameLogService>();
            services.AddTransient<ISeedDataService, SeedDataService>();
        }
    }
}
