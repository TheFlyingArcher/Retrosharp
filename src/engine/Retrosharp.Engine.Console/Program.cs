using Mapster;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NServiceBus;
using NServiceBus.Transport;
using Npgsql;
using Retrosharp.Configuration;
using Retrosharp.Data.Context;
using Retrosharp.DI;
using Retrosharp.Engine.Console.Saga;

namespace Retrosharp.Engine.Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            var config = RetrosharpConfiguration.Instance();
            var messagingConfig = MessagingConfiguration.Instance();

            builder.Services.AddMapster();
            builder.Services.AddDbContext<RetrosharpContext>(b => b.UseNpgsql(config.ConnectionString));

            await ContainerRegistration.RegisterContainer(builder.Services, typeof(Program).Assembly);

            var endpointConfiguration = new EndpointConfiguration(messagingConfig.EndpointName);
            endpointConfiguration.UseSerialization<SystemJsonSerializer>();

            // EnableInstallers() IS needed: found live running this endpoint against a
            // genuinely fresh RabbitMQ broker (Step 8's Docker Compose stack, no queues from
            // any prior run) for the first time -- every earlier "queues form correctly without
            // it" test in this project ran against an already-warm broker with the queue
            // already created from earlier testing, which never actually exercised true
            // first-run behavior. Without this, BrokerVerifier's own startup validation
            // (checking the input queue's delivery-limit policy) throws outright, since it
            // expects the queue to already exist rather than creating it as needed. The one
            // known side effect -- re-running NServiceBus.Persistence.Sql's own schema
            // installer, duplicating what Retrosharp.Data.Migration already applies -- is
            // harmless, since those scripts are idempotent (guarded by "if not exists" checks).
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.UseTransport(new RabbitMQTransport(
                RoutingTopology.Conventional(QueueType.Classic),
                messagingConfig.RabbitMQConnectionString));

            var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
            var dialect = persistence.SqlDialect<SqlDialect.PostgreSql>();
            dialect.JsonBParameterModifier(a => { 
                var param = (NpgsqlParameter)a;
                param.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Jsonb;
            });
            dialect.Schema(messagingConfig.SqlPersistenceSchema);
            persistence.TablePrefix(messagingConfig.SqlPersistenceTablePrefix);
            persistence.ConnectionBuilder(() => new NpgsqlConnection(config.ConnectionString));

            endpointConfiguration.SendFailedMessagesTo(messagingConfig.ErrorQueue);
            endpointConfiguration.AuditProcessedMessagesTo(messagingConfig.AuditQueue);

            var recoverability = endpointConfiguration.Recoverability();
            recoverability.Immediate(immediate => immediate.NumberOfRetries(messagingConfig.ImmediateRetries));
            recoverability.Delayed(delayed => delayed.NumberOfRetries(0));
            recoverability.CustomPolicy((recoverabilityConfig, errorContext) =>
                EngineRecoverabilityPolicy.Decide(
                    messagingConfig,
                    recoverabilityConfig.Failed.ErrorQueue,
                    errorContext.Exception,
                    errorContext.ImmediateProcessingFailures,
                    errorContext.DelayedDeliveriesPerformed));

            builder.UseNServiceBus(endpointConfiguration);

            var host = builder.Build();

            // A separate, minimal WebApplication rather than extending the NServiceBus host
            // itself -- this stays a plain worker process with no other web-hosting surface,
            // just a sidecar liveness listener for Docker Compose's HEALTHCHECK to probe (this
            // process has no HTTP endpoint otherwise). Confirms the process is up and its host
            // finished building, not RabbitMQ/Postgres connectivity specifically -- NServiceBus
            // itself will fail fast on startup if either is unreachable.
            var healthCheckPort = Environment.GetEnvironmentVariable("HEALTH_CHECK_PORT") ?? "8081";
            var healthBuilder = WebApplication.CreateBuilder();
            healthBuilder.WebHost.UseUrls($"http://0.0.0.0:{healthCheckPort}");
            healthBuilder.Logging.ClearProviders();
            var healthApp = healthBuilder.Build();
            healthApp.MapGet("/health", () => Results.Ok());

            // Task.WhenAll would silently mask a startup failure here: healthApp.RunAsync()
            // never completes on its own, so if host.RunAsync() faults immediately (NServiceBus
            // failing to start), WhenAll would just hang forever waiting on the still-running
            // health app -- Docker's HEALTHCHECK would keep reporting healthy on a process that
            // never actually finished starting and is silently not processing any messages.
            // Found live: RestartCount stayed 0 and /health kept responding 200 even after
            // NServiceBus's host startup threw. WhenAny + awaiting the first-completed task
            // rethrows that fault immediately instead, so the process exits and Docker Compose's
            // restart policy can actually recover it.
            var runningHost = host.RunAsync();
            var runningHealthApp = healthApp.RunAsync();
            await await Task.WhenAny(runningHost, runningHealthApp);
        }

    }
}
