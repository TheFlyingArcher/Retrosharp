using Mapster;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using NServiceBus.Persistence.Sql;
using Retrosharp.Configuration;
using Retrosharp.Data.Context;
using Retrosharp.DI;
using Retrosharp.Service.Interface;

namespace Retrosharp.Data.Migrator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            var config = RetrosharpConfiguration.Instance();
            builder.Services.AddMapster();
            builder.Services.AddDbContext<RetrosharpContext>(b => b.UseSqlServer(config.ConnectionString,
                options => options.MigrationsAssembly("Retrosharp.Data.Migration")));
            await ContainerRegistration.RegisterContainer(builder.Services, typeof(Program).Assembly);

            var host = builder.Build();

            // Apply migrations (schema, plus League's HasData seed) and seed Franchise/Ballpark
            // from their reference CSV files. Both steps are idempotent and safe to run on
            // every startup, whether invoked manually, from a GitHub Actions step, or as part
            // of a Docker container's initialization. See spec/seed-data.md.
            using (var scope = host.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<RetrosharpContext>();
                await dbContext.Database.MigrateAsync();
                Console.WriteLine("Database migrations applied successfully.");

                var seedDataService = scope.ServiceProvider.GetRequiredService<ISeedDataService>();
                var seedResult = await seedDataService.SeedAsync();
                Console.WriteLine(
                    $"Seed data applied: {seedResult.FranchisesAdded} franchises added ({seedResult.FranchisesSkipped} already present), " +
                    $"{seedResult.BallparksAdded} ballparks added ({seedResult.BallparksSkipped} already present).");
            }

            await ApplyMessagingSchemaAsync(config);
        }

        /// <summary>
        /// Applies the NServiceBus.Persistence.Sql schema (outbox, subscription, timeout, and
        /// per-saga tables) generated at build time by Retrosharp.Engine.Console, via the
        /// scripts copied into NServiceBusScripts (see the .csproj). The scripts are idempotent
        /// (guarded with "if not exists" checks), so this is safe to run on every startup
        /// alongside the EF Core migrations and seeding above. See spec/phase-1-build-plan.md
        /// Step 4. Not run through NServiceBus's own installers to avoid two mechanisms
        /// competing to create the same tables.
        /// </summary>
        private static async Task ApplyMessagingSchemaAsync(RetrosharpConfiguration config)
        {
            var messagingConfig = MessagingConfiguration.Instance();
            var scriptDirectory = Path.Combine(AppContext.BaseDirectory, "NServiceBusScripts");
            if (!Directory.Exists(scriptDirectory))
            {
                Console.WriteLine($"No NServiceBus persistence scripts found at '{scriptDirectory}'; skipping.");
                return;
            }

            var dialect = new SqlDialect.MsSqlServer();

            await ScriptRunner.Install(
                dialect,
                messagingConfig.SqlPersistenceTablePrefix,
                () => new SqlConnection(config.ConnectionString),
                scriptDirectory,
                true,
                true,
                true,
                CancellationToken.None);

            Console.WriteLine("NServiceBus persistence schema applied successfully.");
        }
    }
}