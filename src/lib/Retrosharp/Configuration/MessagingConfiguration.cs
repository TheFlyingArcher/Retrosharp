using Microsoft.Extensions.Configuration;

namespace Retrosharp.Configuration
{
    /// <summary>
    /// Configuration for the NServiceBus/RabbitMQ messaging infrastructure shared by
    /// Retrosharp.Engine.Console (receiving endpoint) and Retrosharp.UI.Api (send-only endpoint).
    /// See spec/parser.md and spec/phase-1-build-plan.md Step 4.
    /// </summary>
    public sealed class MessagingConfiguration
    {
        public MessagingConfiguration()
        {
            RabbitMQConnectionString = string.Empty;
            EndpointName = "Retrosharp.Engine";
            ErrorQueue = "Retrosharp.Engine.Errors";
            AuditQueue = "Retrosharp.Engine.Audit";
            SqlPersistenceSchema = "dbo";
            SqlPersistenceTablePrefix = string.Empty;
            ImmediateRetries = 3;
            DelayedRetries = 5;
            InitialRetryDelaySeconds = 2;
        }

        public string RabbitMQConnectionString { get; set; }

        public string EndpointName { get; set; }

        public string ErrorQueue { get; set; }

        public string AuditQueue { get; set; }

        /// <summary>
        /// Must match the value Retrosharp.Data.Migration uses when applying the
        /// NServiceBus.Persistence.Sql-generated schema scripts, or the endpoint will look
        /// for saga/outbox/timeout tables under a different name than what was created.
        /// </summary>
        public string SqlPersistenceSchema { get; set; }

        public string SqlPersistenceTablePrefix { get; set; }

        /// <summary>
        /// Number of immediate (no-delay) retries before delayed retries begin.
        /// </summary>
        public int ImmediateRetries { get; set; }

        /// <summary>
        /// Number of delayed retries, with exponential-backoff-with-jitter delay between
        /// each, before the message is moved to the error queue.
        /// </summary>
        public int DelayedRetries { get; set; }

        public int InitialRetryDelaySeconds { get; set; }

        public static MessagingConfiguration Instance()
        {
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var config = configBuilder.Build();
            var section = config.GetSection("Messaging");

            return new MessagingConfiguration
            {
                RabbitMQConnectionString = config.GetConnectionString("RabbitMQ") ?? string.Empty,
                EndpointName = section["EndpointName"] ?? "Retrosharp.Engine",
                ErrorQueue = section["ErrorQueue"] ?? "Retrosharp.Engine.Errors",
                AuditQueue = section["AuditQueue"] ?? "Retrosharp.Engine.Audit",
                SqlPersistenceSchema = section["SqlPersistenceSchema"] ?? "dbo",
                SqlPersistenceTablePrefix = section["SqlPersistenceTablePrefix"] ?? string.Empty,
                ImmediateRetries = int.TryParse(section["ImmediateRetries"], out var immediateRetries) ? immediateRetries : 3,
                DelayedRetries = int.TryParse(section["DelayedRetries"], out var delayedRetries) ? delayedRetries : 5,
                InitialRetryDelaySeconds = int.TryParse(section["InitialRetryDelaySeconds"], out var initialDelay) ? initialDelay : 2,
            };
        }
    }
}
