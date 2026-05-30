using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Retrosharp.Data.Context;
namespace Retrosharp.Data
{
    public class IocRegistratrions
    {
        public static void Register(IServiceCollection services)
        {
            services.AddDbContext<RetrosharpContext>(b => b.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Retrosharp;Trusted_Connection=true;"));
        }
    }
}
