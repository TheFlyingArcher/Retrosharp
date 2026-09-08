using System.IO.Compression;

using Retrosharp.Engine.Console.Saga;

namespace Retrosharp.Engine.Console.Tests
{
    /// <summary>
    /// Tests for <see cref="GameLogArchive"/> (Step 11c, see spec/retrosheet-auto-download.md).
    /// </summary>
    public sealed class GameLogArchiveTests : IDisposable
    {
        private readonly string _tempRoot;

        public GameLogArchiveTests()
        {
            _tempRoot = Path.Combine(Path.GetTempPath(), "retrosharp-gamelog-archive-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempRoot);
        }

        public void Dispose()
        {
            try { if (Directory.Exists(_tempRoot)) Directory.Delete(_tempRoot, recursive: true); }
            catch { /* best effort */ }
        }

        [Theory]
        [InlineData("gl2024.txt", true)]
        [InlineData("GL2024.TXT", true)]     // Retrosheet ships it upper-cased
        [InlineData("gl1871.txt", true)]
        [InlineData("gl2024.zip", false)]
        [InlineData("2024SDN.EVN", false)]
        [InlineData("gl24.txt", false)]      // two-digit year
        [InlineData("glabcd.txt", false)]
        [InlineData("readme.txt", false)]
        [InlineData("", false)]
        public void IsGameLogFile(string name, bool expected) =>
            Assert.Equal(expected, GameLogArchive.IsGameLogFile(name));

        [Fact]
        public void SeasonOf_ReadsTheFourDigitsAfterThePrefix() =>
            Assert.Equal(2024, GameLogArchive.SeasonOf("GL2024.TXT"));

        [Fact]
        public void Extract_WritesTheSingleGameLog_Flattened()
        {
            var zipPath = CreateArchive("gl2024.zip", ("nested/GL2024.TXT", "20240328,0,Thu,..."));
            var target = Path.Combine(_tempRoot, "out");

            var path = GameLogArchive.Extract(zipPath, target);

            Assert.Equal(Path.Combine(target, "GL2024.TXT"), path);
            Assert.Equal("20240328,0,Thu,...", File.ReadAllText(path));
            Assert.False(Directory.Exists(Path.Combine(target, "nested")));
        }

        [Fact]
        public void Extract_IgnoresNonGameLogEntries()
        {
            var zipPath = CreateArchive("gl2024.zip",
                ("GL2024.TXT", "games"),
                ("readme.txt", "junk"),
                ("TEAM2024", "teams"));
            var target = Path.Combine(_tempRoot, "out");

            var path = GameLogArchive.Extract(zipPath, target);

            Assert.Equal("games", File.ReadAllText(path));
            Assert.False(File.Exists(Path.Combine(target, "readme.txt")));
        }

        [Fact]
        public void Extract_OverwritesAnExistingFile()
        {
            var target = Path.Combine(_tempRoot, "out");
            Directory.CreateDirectory(target);
            File.WriteAllText(Path.Combine(target, "GL2024.TXT"), "stale");
            var zipPath = CreateArchive("gl2024.zip", ("GL2024.TXT", "fresh"));

            var path = GameLogArchive.Extract(zipPath, target);

            Assert.Equal("fresh", File.ReadAllText(path));
        }

        [Fact]
        public void Extract_NoGameLogEntry_ThrowsInvalidData()
        {
            var zipPath = CreateArchive("gl2024.zip", ("readme.txt", "junk"), ("2024SDN.EVN", "x"));

            var ex = Assert.Throws<InvalidDataException>(() => GameLogArchive.Extract(zipPath, Path.Combine(_tempRoot, "out")));
            Assert.Contains("no Retrosheet game-log file", ex.Message);
        }

        [Fact]
        public void Extract_MultipleGameLogEntries_ThrowsInvalidData()
        {
            var zipPath = CreateArchive("gl.zip", ("gl2024.txt", "a"), ("gl2023.txt", "b"));

            var ex = Assert.Throws<InvalidDataException>(() => GameLogArchive.Extract(zipPath, Path.Combine(_tempRoot, "out")));
            Assert.Contains("more than one game-log file", ex.Message);
        }

        [Fact]
        public void Extract_MissingArchive_Throws() =>
            Assert.ThrowsAny<Exception>(() => GameLogArchive.Extract(Path.Combine(_tempRoot, "nope.zip"), _tempRoot));

        private string CreateArchive(string name, params (string EntryName, string Content)[] entries)
        {
            var zipPath = Path.Combine(_tempRoot, name);
            using var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create);
            foreach (var (entryName, content) in entries)
            {
                using var stream = zip.CreateEntry(entryName).Open();
                using var writer = new StreamWriter(stream);
                writer.Write(content);
            }
            return zipPath;
        }
    }
}
