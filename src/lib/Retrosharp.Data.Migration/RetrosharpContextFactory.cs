using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Retrosharp.Configuration;
using Retrosharp.Data.Context;

namespace Retrosharp.Data.Migrator
{
    /// <summary>
    /// Design-time factory for creating RetrosharpContext instances during EF Core migrations.
    /// </summary>
    public class RetrosharpContextFactory : IDesignTimeDbContextFactory<RetrosharpContext>
    {
        public RetrosharpContext CreateDbContext(string[] args)
        {
            var config = RetrosharpConfiguration.Instance();

            var optionsBuilder = new DbContextOptionsBuilder<RetrosharpContext>();
            optionsBuilder.UseSqlServer(config.ConnectionString, 
                b => b.MigrationsAssembly("Retrosharp.Data.Migration"));

            return new RetrosharpContext(optionsBuilder.Options);
        }
    }
}
