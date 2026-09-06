using Microsoft.Extensions.Configuration;

namespace Retrosharp.Configuration
{
    /// <summary>
    /// Tuning for the bulk Game Event import saga in Retrosharp.Engine.Console. Bound from the
    /// <c>BulkImport</c> section of <c>appsettings.json</c> (or <c>BulkImport__*</c> environment
    /// variables), the same convention as <see cref="MessagingConfiguration"/>. See
    /// spec/bulk-import.md.
    /// </summary>
    public sealed class BulkImportConfiguration
    {
        public BulkImportConfiguration()
        {
            DefaultBatchSize = 10;
            WatchdogTimeoutHours = 6;
            ExtractionRoot = string.Empty;
        }

        /// <summary>
        /// Files dispatched concurrently when a bulk import request does not specify its own
        /// batch size.
        /// </summary>
        public int DefaultBatchSize { get; set; }

        /// <summary>
        /// How long the saga waits for a file's completion or failure signal before its
        /// watchdog gives up, marks every still-unfinished file failed, and closes the run.
        /// The backstop for the engine crashing mid-file or a transient error retrying past
        /// this point.
        /// </summary>
        public int WatchdogTimeoutHours { get; set; }

        /// <summary>
        /// Directory the archive's event files are extracted into (a per-run subdirectory is
        /// created beneath it). Empty means "an <c>_bulk-import/&lt;trackingId&gt;/</c>
        /// subdirectory next to the source zip".
        /// </summary>
        public string ExtractionRoot { get; set; }

        public static BulkImportConfiguration Instance()
        {
            // See the matching comment in MessagingConfiguration.Instance() -- both environment
            // variables are checked because config is shared between a generic-host console app
            // and an ASP.NET Core web app, which each use a different one by convention.
            var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                ?? "Production";

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var config = configBuilder.Build();
            var section = config.GetSection("BulkImport");

            return new BulkImportConfiguration
            {
                DefaultBatchSize = int.TryParse(section["DefaultBatchSize"], out var batchSize) && batchSize > 0 ? batchSize : 10,
                WatchdogTimeoutHours = int.TryParse(section["WatchdogTimeoutHours"], out var hours) && hours > 0 ? hours : 6,
                ExtractionRoot = section["ExtractionRoot"] ?? string.Empty
            };
        }
    }
}
