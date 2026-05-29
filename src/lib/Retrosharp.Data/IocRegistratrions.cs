using Microsoft.Extensions.DependencyInjection;

using Retrosharp.Data.Context;
namespace Retrosharp.Data
{
    public class IocRegistratrions
    {
        public static void Register(IServiceCollection services)
        {
            services.AddTransient<IRetrosharpContext, RetrosharpContext>();
        }
    }
}
