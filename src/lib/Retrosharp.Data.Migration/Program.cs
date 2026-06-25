using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Retrosharp.Configuration;
using Retrosharp.Data.Context;
using Retrosharp.DI;

var builder = Host.CreateApplicationBuilder(args);
var config = RetrosharpConfiguration.Instance();
builder.Services.AddMapster();
builder.Services.AddDbContext<RetrosharpContext>(b => b.UseSqlServer(config.ConnectionString));
await ContainerRegistration.RegisterContainer(builder.Services, typeof(Program).Assembly);

Console.WriteLine("Hello, World!");

await builder.Build().RunAsync();