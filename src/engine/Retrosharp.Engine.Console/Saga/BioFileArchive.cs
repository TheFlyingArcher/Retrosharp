using System.IO.Compression;
using System.Text.RegularExpressions;

namespace Retrosharp.Engine.Console.Saga
{
    /// <summary>
    /// Pulls the biographical-data file (<c>biofile0.csv</c> -- the newer 32-column format,
    /// see spec/person.md) out of a downloaded Retrosheet <c>biodata.zip</c>. That archive
    /// also carries the legacy <c>biofile.csv</c> plus six unrelated files (coaches, umpires,
    /// managers, teams, ballparks, relatives); only <c>biofile0.csv</c> is extracted. The
    /// counterpart to <see cref="GameLogArchive"/> for the Person import path. See
    /// spec/retrosheet-auto-download.md.
    /// </summary>
    internal static partial class BioFileArchive
    {
        [GeneratedRegex(@"^biofile0\.csv$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex BioFileNameRegex();

        /// <summary>
        /// True if <paramref name="name"/> is the newer-format biofile entry name
        /// (<c>biofile0.csv</c>), not the legacy <c>biofile.csv</c>.
        /// </summary>
        public static bool IsBioFile(string name) =>
            !string.IsNullOrEmpty(name) && BioFileNameRegex().IsMatch(name);

        /// <summary>
        /// Extracts <c>biofile0.csv</c> from the archive at <paramref name="zipPath"/> into
        /// <paramref name="targetDir"/> (created if needed, any directory structure flattened,
        /// an existing file overwritten) and returns its local path. Throws
        /// <see cref="InvalidDataException"/> if the archive does not contain exactly one
        /// <c>biofile0.csv</c>, and rethrows the framework's exception if the file is missing
        /// or is not a readable zip.
        /// </summary>
        public static string Extract(string zipPath, string targetDir)
        {
            using var archive = ZipFile.OpenRead(zipPath);

            var matches = archive.Entries.Where(e => IsBioFile(e.Name)).ToList();

            if (matches.Count == 0)
                throw new InvalidDataException(
                    $"The archive '{zipPath}' contains no biofile0.csv (the newer-format Retrosheet biographical-data file).");

            if (matches.Count > 1)
                throw new InvalidDataException(
                    $"The archive '{zipPath}' contains more than one biofile0.csv; expected exactly one.");

            Directory.CreateDirectory(targetDir);
            var entry = matches[0];
            var destination = Path.Combine(targetDir, entry.Name);
            entry.ExtractToFile(destination, overwrite: true);

            return destination;
        }
    }
}
