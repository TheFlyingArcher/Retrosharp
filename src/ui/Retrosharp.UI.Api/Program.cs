using Mapster;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using NServiceBus;
using Retrosharp.Configuration;
using Retrosharp.DI;
using Retrosharp.Message.Diagnostics;
using Retrosharp.Message.GameLog;
using Retrosharp.Message.Person;

var builder = WebApplication.CreateBuilder(args);

await ContainerRegistration.RegisterContainer(builder.Services, typeof(Program).Assembly);

builder.Services.AddMapster();

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

builder.UseNServiceBus(endpointConfiguration);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
