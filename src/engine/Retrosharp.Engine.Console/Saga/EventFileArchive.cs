using System.IO.Compression;
using System.Text.RegularExpressions;

namespace Retrosharp.Engine.Console.Saga
{
    /// <summary>
    /// Reads and extracts Retrosheet team-season play-by-play event files
    /// (<c>20YYTTT.EVN</c>/<c>.EVA</c>, year-first -- see spec/game-event.md) from a season's
    /// zip archive, and resolves the single season an archive belongs to. Used by
    /// <see cref="BulkGameEventImportSaga"/>; the name/season parsing here is pure and unit
    /// tested independently of the saga. See spec/bulk-import.md.
    /// </summary>
    internal static partial class EventFileArchive
    {
        [GeneratedRegex(@"^\d{4}[A-Za-z]{3}\.EV[AN]$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex EventFileNameRegex();

        // Retrosheet's earliest event data is 1871; the upper bound is just a sanity ceiling.
        private const int EarliestSeason = 1871;
        private const int LatestSeason = 2100;

        /// <summary>
        /// True if <paramref name="name"/> is a Retrosheet event file name (name only, no
        /// directory): four digits, three letters, then <c>.EVN</c> or <c>.EVA</c>.
        /// </summary>
        public static bool IsEventFile(string name) =>
            !string.IsNullOrEmpty(name) && EventFileNameRegex().IsMatch(name);

        /// <summary>
        /// The season year encoded in the first four characters of an event file name.
        /// </summary>
        public static short SeasonOf(string eventFileName) =>
            short.Parse(eventFileName.AsSpan(0, 4), provider: System.Globalization.CultureInfo.InvariantCulture);

        /// <summary>
        /// Lists the distinct Retrosheet event file names in the archive at
        /// <paramref name="zipPath"/> (flattened -- directory structure inside the archive is
        /// ignored, non-event entries are skipped). Throws if the file is missing or is not a
        /// readable zip.
        /// </summary>
        public static IReadOnlyList<string> ListEventFiles(string zipPath)
        {
            using var archive = ZipFile.OpenRead(zipPath);
            return archive.Entries
                .Select(e => e.Name)
                .Where(IsEventFile)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Extracts just the named event files from the archive into <paramref name="targetDir"/>
        /// (created if needed), flattening any directory structure and overwriting existing
        /// files. Returns the names actually written.
        /// </summary>
        public static IReadOnlyList<string> ExtractFiles(string zipPath, string targetDir, IEnumerable<string> fileNames)
        {
            var wanted = new HashSet<string>(fileNames, StringComparer.OrdinalIgnoreCase);
            Directory.CreateDirectory(targetDir);

            var written = new List<string>();
            using var archive = ZipFile.OpenRead(zipPath);
            foreach (var entry in archive.Entries)
            {
                if (!wanted.Contains(entry.Name) || written.Contains(entry.Name))
                    continue;

                entry.ExtractToFile(Path.Combine(targetDir, entry.Name), overwrite: true);
                written.Add(entry.Name);
            }

            return written;
        }

        /// <summary>
        /// Resolves the single season an archive's event files all belong to. Returns false
        /// (with <paramref name="error"/> set) when there are no event files, when they span
        /// more than one season, or when the season is outside the plausible range.
        /// </summary>
        public static bool TryResolveSeason(IEnumerable<string> eventFileNames, out short season, out string? error)
        {
            season = 0;

            var seasons = eventFileNames.Where(IsEventFile).Select(SeasonOf).Distinct().OrderBy(y => y).ToList();
            if (seasons.Count == 0)
            {
                error = "The archive contains no Retrosheet game event files (expected names like 2024SDN.EVN or 2024SEA.EVA).";
                return false;
            }

            if (seasons.Count > 1)
            {
                error = $"The archive spans multiple seasons ({string.Join(", ", seasons)}); bulk import handles one season per archive.";
                return false;
            }

            season = seasons[0];
            if (season is < EarliestSeason or > LatestSeason)
            {
                error = $"The archive's event files report an implausible season ({season}).";
                season = 0;
                return false;
            }

            error = null;
            return true;
        }
    }
}
