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
            // DOTNET_ENVIRONMENT is what Host.CreateApplicationBuilder (Retrosharp.Engine.Console)
            // reads; ASPNETCORE_ENVIRONMENT is what WebApplication.CreateBuilder (Retrosharp.UI.Api)
            // reads. This is called via its own ConfigurationBuilder rather than the host's
            // builder.Configuration, so it has to check both to layer the same
            // appsettings.{Environment}.json override each host already applies to everything else.
            var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                ?? "Production";

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var config = configBuilder.Build();
            return new RetrosharpConfiguration
            {
                ConnectionString = config.GetConnectionString("DefaultConnection")
            };
        }
    }
}
