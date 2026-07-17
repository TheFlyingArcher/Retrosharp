using Mapster;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using NServiceBus.Transport;
using Retrosharp.Configuration;
using Retrosharp.Data.Context;
using Retrosharp.DI;

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
            builder.Services.AddDbContext<RetrosharpContext>(b => b.UseSqlServer(config.ConnectionString));

            await ContainerRegistration.RegisterContainer(builder.Services, typeof(Program).Assembly);

            var endpointConfiguration = new EndpointConfiguration(messagingConfig.EndpointName);
            endpointConfiguration.UseSerialization<SystemJsonSerializer>();

            // No EnableInstallers() call: the RabbitMQ transport declares its own
            // queues/exchanges unconditionally during normal startup regardless of installer
            // settings (confirmed empirically -- queues formed correctly without it, and the
            // transport's own docs never mention installers at all). EnableInstallers() would
            // only have re-run NServiceBus.Persistence.Sql's own schema installer, duplicating
            // what Retrosharp.Data.Migration already applies alongside its EF Core migrations
            // (see spec/seed-data.md's "one command prepares the database" precedent).
            endpointConfiguration.UseTransport(new RabbitMQTransport(
                RoutingTopology.Conventional(QueueType.Classic),
                messagingConfig.RabbitMQConnectionString));

            var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
            var dialect = persistence.SqlDialect<SqlDialect.MsSqlServer>();
            dialect.Schema(messagingConfig.SqlPersistenceSchema);
            persistence.TablePrefix(messagingConfig.SqlPersistenceTablePrefix);
            persistence.ConnectionBuilder(() => new SqlConnection(config.ConnectionString));

            endpointConfiguration.SendFailedMessagesTo(messagingConfig.ErrorQueue);
            endpointConfiguration.AuditProcessedMessagesTo(messagingConfig.AuditQueue);

            var recoverability = endpointConfiguration.Recoverability();
            recoverability.Immediate(immediate => immediate.NumberOfRetries(messagingConfig.ImmediateRetries));
            recoverability.Delayed(delayed => delayed.NumberOfRetries(0));
            recoverability.CustomPolicy((recoverabilityConfig, errorContext) =>
                ExponentialBackoffWithJitterPolicy(messagingConfig, recoverabilityConfig, errorContext));

            builder.UseNServiceBus(endpointConfiguration);

            var host = builder.Build();
            await host.RunAsync();
        }

        /// <summary>
        /// NServiceBus's built-in delayed recoverability only supports a linear TimeIncrease, not
        /// true exponential backoff or jitter, so parser.md's "exponential backoff with jitter"
        /// requirement is implemented here instead of via config alone. Immediate retries are left
        /// to the built-in Immediate() policy; this only governs the delayed-retry phase.
        /// </summary>
        private static RecoverabilityAction ExponentialBackoffWithJitterPolicy(
            MessagingConfiguration messagingConfig,
            RecoverabilityConfig recoverabilityConfig,
            ErrorContext errorContext)
        {
            if (errorContext.ImmediateProcessingFailures < messagingConfig.ImmediateRetries)
                return RecoverabilityAction.ImmediateRetry();

            if (errorContext.DelayedDeliveriesPerformed < messagingConfig.DelayedRetries)
            {
                var attempt = errorContext.DelayedDeliveriesPerformed + 1;
                var baseDelaySeconds = messagingConfig.InitialRetryDelaySeconds * Math.Pow(2, attempt - 1);
                var jitterMilliseconds = Random.Shared.Next(0, (int)(baseDelaySeconds * 1000 * 0.2));
                var delay = TimeSpan.FromSeconds(baseDelaySeconds) + TimeSpan.FromMilliseconds(jitterMilliseconds);

                return RecoverabilityAction.DelayedRetry(delay);
            }

            return RecoverabilityAction.MoveToError(recoverabilityConfig.Failed.ErrorQueue);
        }
    }
}
