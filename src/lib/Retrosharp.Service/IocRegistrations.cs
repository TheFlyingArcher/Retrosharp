using System;
using System.Collections.Generic;
using System.Text;

using Mapster;

using Microsoft.Extensions.DependencyInjection;

using DI = Retrosharp.DI;

using Retrosharp.Format;
using Retrosharp.Service.ETL;
using Retrosharp.Service.Interface.ETL;

namespace Retrosharp.Service
{
    public class IocRegistrations : DI.IRegister
    {
        public async Task Register(IServiceCollection services)
        {
            services.AddTransient<IRetrosheetFileService<GameLog>, GameLogFileService>();
            services.AddTransient<IRetrosheetFileService<BioFile>, BioFileService>();
        }
    }
}
