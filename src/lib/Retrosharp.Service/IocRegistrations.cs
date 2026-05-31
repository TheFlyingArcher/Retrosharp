using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.DependencyInjection;

using Retrosharp.Format;
using Retrosharp.Service.ETL;
using Retrosharp.Service.Interface.ETL;

namespace Retrosharp.Service
{
    public class IocRegistrations
    {
        public void Register(IServiceCollection services)
        {
            services.AddTransient<IRetrosheetFileService<GameLog>, GameLogFileService>();
            services.AddTransient<IRetrosheetFileService<BioFile>, BioFileService>();
        }
    }
}
