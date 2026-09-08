using System.IO.Compression;
using System.Text.RegularExpressions;

namespace Retrosharp.Engine.Console.Saga
{
    /// <summary>
    /// Pulls the single Retrosheet season game-log file (<c>glYYYY.txt</c> -- CSV despite the
    /// extension, see spec/game-log.md) out of a downloaded <c>glYYYY.zip</c>. The counterpart
    /// to <see cref="EventFileArchive"/> for the Game Log import path; name parsing here is
    /// pure and unit tested independently of the saga. See spec/retrosheet-auto-download.md.
    /// </summary>
    internal static partial class GameLogArchive
    {
        [GeneratedRegex(@"^gl\d{4}\.txt$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex GameLogFileNameRegex();

        /// <summary>
        /// True if <paramref name="name"/> is a Retrosheet season game-log file name (name
        /// only, no directory): <c>gl</c>, four digits, then <c>.txt</c>.
        /// </summary>
        public static bool IsGameLogFile(string name) =>
            !string.IsNullOrEmpty(name) && GameLogFileNameRegex().IsMatch(name);

        /// <summary>
        /// The season year encoded in characters 3-6 of a game-log file name
        /// (<c>gl<b>2024</b>.txt</c>).
        /// </summary>
        public static short SeasonOf(string gameLogFileName) =>
            short.Parse(gameLogFileName.AsSpan(2, 4), provider: System.Globalization.CultureInfo.InvariantCulture);

        /// <summary>
        /// Extracts the one game-log file from the archive at <paramref name="zipPath"/> into
        /// <paramref name="targetDir"/> (created if needed, directory structure inside the
        /// archive flattened, an existing file overwritten) and returns its local path. Throws
        /// <see cref="InvalidDataException"/> if the archive does not contain exactly one
        /// game-log file, and rethrows the framework's exception if the file is missing or is
        /// not a readable zip.
        /// </summary>
        public static string Extract(string zipPath, string targetDir)
        {
            using var archive = ZipFile.OpenRead(zipPath);

            var matches = archive.Entries
                .Where(e => IsGameLogFile(e.Name))
                .ToList();

            if (matches.Count == 0)
                throw new InvalidDataException(
                    $"The archive '{zipPath}' contains no Retrosheet game-log file (expected an entry named like gl2024.txt).");

            if (matches.Count > 1)
                throw new InvalidDataException(
                    $"The archive '{zipPath}' contains more than one game-log file ({string.Join(", ", matches.Select(e => e.Name))}); expected exactly one.");

            Directory.CreateDirectory(targetDir);
            var entry = matches[0];
            var destination = Path.Combine(targetDir, entry.Name);
            entry.ExtractToFile(destination, overwrite: true);

            return destination;
        }
    }
}
