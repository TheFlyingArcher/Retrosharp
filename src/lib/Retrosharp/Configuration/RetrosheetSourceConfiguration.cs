using System.Globalization;

using Microsoft.Extensions.Configuration;

namespace Retrosharp.Configuration
{
    /// <summary>
    /// Where and how Retrosharp.Engine.Console fetches Retrosheet source archives (season game
    /// logs and event archives) and where it stages them on disk. Bound from the
    /// <c>RetrosheetSource</c> section of <c>appsettings.json</c> (or <c>RetrosheetSource__*</c>
    /// environment variables), the same convention as <see cref="MessagingConfiguration"/> and
    /// <see cref="BulkImportConfiguration"/>. See spec/retrosheet-auto-download.md.
    /// </summary>
    public sealed class RetrosheetSourceConfiguration
    {
        /// <summary>The token replaced with the season year in the path templates.</summary>
        public const string SeasonToken = "{season}";

        public RetrosheetSourceConfiguration()
        {
            BaseUrl = "https://www.retrosheet.org/";
            GameLogArchivePath = "gamelogs/gl{season}.zip";
            EventArchivePath = "events/{season}eve.zip";
            // The "Download biofile.zip" link on retrosheet.org/biofile.htm actually points
            // here; the archive holds eight files, of which biofile0.csv (newer 32-column
            // format) is the one the Person parser reads.
            BioFileArchivePath = "downloads/biodata.zip";
            WorkingRoot = string.Empty;
            HttpTimeoutSeconds = 100;
        }

        /// <summary>Root the archive paths below are resolved against.</summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Path template for a season game-log archive, relative to <see cref="BaseUrl"/>.
        /// <see cref="SeasonToken"/> is substituted with the season year.
        /// </summary>
        public string GameLogArchivePath { get; set; }

        /// <summary>
        /// Path template for a season play-by-play event archive, relative to
        /// <see cref="BaseUrl"/>. <see cref="SeasonToken"/> is substituted with the season year.
        /// </summary>
        public string EventArchivePath { get; set; }

        /// <summary>
        /// Path for the biographical-data archive, relative to <see cref="BaseUrl"/>. Has no
        /// season component.
        /// </summary>
        public string BioFileArchivePath { get; set; }

        /// <summary>
        /// Parent directory for each import's per-run working directory (the downloaded zip
        /// plus everything extracted from it). Empty means a <c>retrosharp-import/</c> folder
        /// under the system temp directory -- see <see cref="ResolvedWorkingRoot"/>.
        /// </summary>
        public string WorkingRoot { get; set; }

        /// <summary><see cref="System.Net.Http.HttpClient.Timeout"/> for archive downloads.</summary>
        public int HttpTimeoutSeconds { get; set; }

        /// <summary>
        /// <see cref="WorkingRoot"/> when set, otherwise <c>%TEMP%/retrosharp-import</c>.
        /// </summary>
        public string ResolvedWorkingRoot =>
            string.IsNullOrWhiteSpace(WorkingRoot)
                ? Path.Combine(Path.GetTempPath(), "retrosharp-import")
                : WorkingRoot;

        /// <summary>Absolute URL of the game-log archive for <paramref name="season"/>.</summary>
        public Uri GameLogUri(int season) => BuildUri(GameLogArchivePath, season);

        /// <summary>Absolute URL of the event archive for <paramref name="season"/>.</summary>
        public Uri EventArchiveUri(int season) => BuildUri(EventArchivePath, season);

        /// <summary>Absolute URL of the biographical-data archive.</summary>
        public Uri BioFileUri() => BuildUri(BioFileArchivePath, season: null);

        private Uri BuildUri(string pathTemplate, int? season)
        {
            var root = BaseUrl.EndsWith('/') ? BaseUrl : BaseUrl + "/";
            var relative = season is { } year
                ? pathTemplate.Replace(SeasonToken, year.ToString(CultureInfo.InvariantCulture))
                : pathTemplate;

            return new Uri(new Uri(root, UriKind.Absolute), relative);
        }

        public static RetrosheetSourceConfiguration Instance()
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
            var section = config.GetSection("RetrosheetSource");
            var defaults = new RetrosheetSourceConfiguration();

            return new RetrosheetSourceConfiguration
            {
                BaseUrl = Blank(section["BaseUrl"]) ?? defaults.BaseUrl,
                GameLogArchivePath = Blank(section["GameLogArchivePath"]) ?? defaults.GameLogArchivePath,
                EventArchivePath = Blank(section["EventArchivePath"]) ?? defaults.EventArchivePath,
                BioFileArchivePath = Blank(section["BioFileArchivePath"]) ?? defaults.BioFileArchivePath,
                WorkingRoot = section["WorkingRoot"] ?? string.Empty,
                HttpTimeoutSeconds = int.TryParse(section["HttpTimeoutSeconds"], out var timeout) && timeout > 0
                    ? timeout
                    : defaults.HttpTimeoutSeconds
            };
        }

        private static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
