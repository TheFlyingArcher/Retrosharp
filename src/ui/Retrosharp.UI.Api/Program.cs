using Mapster;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using NServiceBus;
using Retrosharp.Configuration;
using Retrosharp.Data.Context;
using Retrosharp.DI;
using Retrosharp.Message.Diagnostics;
using Retrosharp.Message.GameEvent;
using Retrosharp.Message.GameLog;
using Retrosharp.Message.Person;

var builder = WebApplication.CreateBuilder(args);

await ContainerRegistration.RegisterContainer(builder.Services, typeof(Program).Assembly);

builder.Services.AddMapster();

// UI.Api is send-only (no NServiceBus persistence needs a DbContext here), but
// ContainerRegistration's assembly-wide scan still registers every repository/service --
// including ones that require RetrosharpContext -- so it must be registered too, or ASP.NET
// Core's service-provider validation fails at startup. Found live when verifying Step 6f's
// GameEventController end-to-end (Person/GameLog controllers have the same transitive
// dependency and would have failed identically). See spec/phase-1-build-plan.md Step 6f.
var retrosharpConfig = RetrosharpConfiguration.Instance();
builder.Services.AddDbContext<RetrosharpContext>(b => b.UseNpgsql(retrosharpConfig.ConnectionString));

var messagingConfig = MessagingConfiguration.Instance();

var endpointConfiguration = new EndpointConfiguration("Retrosharp.UI.Api");
endpointConfiguration.UseSerialization<SystemJsonSerializer>();
endpointConfiguration.SendOnly();

var transport = new RabbitMQTransport(
    RoutingTopology.Conventional(QueueType.Classic),
    messagingConfig.RabbitMQConnectionString);
var routing = endpointConfiguration.UseTransport(transport);
routing.RouteToEndpoint(typeof(PingMessage), messagingConfig.EndpointName);
routing.RouteToEndpoint(typeof(FailingPingMessage), messagingConfig.EndpointName);
routing.RouteToEndpoint(typeof(PersonStart), messagingConfig.EndpointName);
routing.RouteToEndpoint(typeof(GameLogStart), messagingConfig.EndpointName);
routing.RouteToEndpoint(typeof(GameEventStart), messagingConfig.EndpointName);

builder.UseNServiceBus(endpointConfiguration);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Docker Compose's healthcheck probes this to know when the API is actually ready to serve
// requests, not just that the process has started -- a real Postgres-connectivity check, not a
// bare liveness ping. See spec/phase-1-build-plan.md Step 8.
builder.Services.AddHealthChecks()
    .AddNpgSql(retrosharpConfig.ConnectionString, name: "postgres");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
