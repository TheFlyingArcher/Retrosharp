using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.Configuration;

namespace Retrosharp.Configuration
{
    public sealed class RetrosharpConfiguration
    {
        public RetrosharpConfiguration()
        {
            ConnectionString = string.Empty;
        }

        public string ConnectionString { get; set; }

        public static RetrosharpConfiguration Instance()
        {
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var config = configBuilder.Build();
            return new RetrosharpConfiguration
            {
                ConnectionString = config.GetConnectionString("DefaultConnection")
            };
        }
    }
}
