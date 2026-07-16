using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        }
    }
}