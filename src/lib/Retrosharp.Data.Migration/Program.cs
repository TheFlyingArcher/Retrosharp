using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Retrosharp.Configuration;
using Retrosharp.Data.Context;
using Retrosharp.DI;

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

            // Apply migrations
            using (var scope = host.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<RetrosharpContext>();
                await dbContext.Database.MigrateAsync();
                Console.WriteLine("Database migrations applied successfully.");
            }

            await host.RunAsync();
        }
    }
}