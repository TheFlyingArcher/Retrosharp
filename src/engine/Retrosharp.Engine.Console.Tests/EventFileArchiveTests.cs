using System.IO.Compression;

using Retrosharp.Engine.Console.Saga;

namespace Retrosharp.Engine.Console.Tests
{
    public sealed class EventFileArchiveTests : IDisposable
    {
        private readonly string _tempRoot;

        public EventFileArchiveTests()
        {
            _tempRoot = Path.Combine(Path.GetTempPath(), "retrosharp-archive-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempRoot);
        }

        public void Dispose()
        {
            try { if (Directory.Exists(_tempRoot)) Directory.Delete(_tempRoot, recursive: true); }
            catch { /* best effort */ }
        }

        [Theory]
        [InlineData("2024SDN.EVN", true)]
        [InlineData("2024SEA.EVA", true)]
        [InlineData("2024sdn.evn", true)]   // case-insensitive
        [InlineData("1998BOS.EVA", true)]
        [InlineData("SDN2024.EVN", false)]  // team-first (old, wrong) ordering
        [InlineData("2024SDN.ROS", false)]  // roster file
        [InlineData("gl2024.txt", false)]
        [InlineData("2024SD.EVN", false)]   // two-letter team
        [InlineData("2024SDNN.EVN", false)] // four-letter team
        [InlineData("readme.txt", false)]
        [InlineData("", false)]
        public void IsEventFile(string name, bool expected) =>
            Assert.Equal(expected, EventFileArchive.IsEventFile(name));

        [Fact]
        public void SeasonOf_ReadsTheLeadingFourDigits() =>
            Assert.Equal(2024, EventFileArchive.SeasonOf("2024SDN.EVN"));

        [Fact]
        public void TryResolveSeason_SingleSeason_Succeeds()
        {
            var ok = EventFileArchive.TryResolveSeason(
                new[] { "2024SDN.EVN", "2024ARI.EVN", "2024SEA.EVA", "readme.txt" }, out var season, out var error);

            Assert.True(ok);
            Assert.Equal(2024, season);
            Assert.Null(error);
        }

        [Fact]
        public void TryResolveSeason_NoEventFiles_Fails()
        {
            var ok = EventFileArchive.TryResolveSeason(new[] { "readme.txt", "TEAM.csv" }, out _, out var error);

            Assert.False(ok);
            Assert.Contains("no Retrosheet game event files", error);
        }

        [Fact]
        public void TryResolveSeason_MultipleSeasons_Fails()
        {
            var ok = EventFileArchive.TryResolveSeason(new[] { "2024SDN.EVN", "2023ARI.EVN" }, out _, out var error);

            Assert.False(ok);
            Assert.Contains("multiple seasons", error);
        }

        [Fact]
        public void TryResolveSeason_ImplausibleYear_Fails()
        {
            var ok = EventFileArchive.TryResolveSeason(new[] { "1850SDN.EVN" }, out _, out var error);

            Assert.False(ok);
            Assert.Contains("implausible season", error);
        }

        [Fact]
        public void ListEventFiles_FlattensAndFiltersAndSorts()
        {
            var zipPath = CreateArchive("season.zip",
                ("2024SDN.EVN", "x"),
                ("2024ARI.EVN", "x"),
                ("notes.txt", "x"),
                ("nested/2024SEA.EVA", "x"));

            var names = EventFileArchive.ListEventFiles(zipPath);

            Assert.Equal(new[] { "2024ARI.EVN", "2024SDN.EVN", "2024SEA.EVA" }, names);
        }

        [Fact]
        public void ExtractFiles_WritesOnlyTheRequestedNames_Flattened()
        {
            var zipPath = CreateArchive("season.zip",
                ("2024SDN.EVN", "sdn"),
                ("2024ARI.EVN", "ari"),
                ("nested/2024SEA.EVA", "sea"),
                ("notes.txt", "junk"));
            var target = Path.Combine(_tempRoot, "out");

            var written = EventFileArchive.ExtractFiles(zipPath, target, new[] { "2024SDN.EVN", "2024SEA.EVA" });

            Assert.Equal(new[] { "2024SDN.EVN", "2024SEA.EVA" }, written.OrderBy(n => n));
            Assert.True(File.Exists(Path.Combine(target, "2024SDN.EVN")));
            Assert.Equal("sea", File.ReadAllText(Path.Combine(target, "2024SEA.EVA")));
            Assert.False(File.Exists(Path.Combine(target, "2024ARI.EVN")));
            Assert.False(File.Exists(Path.Combine(target, "notes.txt")));
            Assert.False(Directory.Exists(Path.Combine(target, "nested")));
        }

        [Fact]
        public void ListEventFiles_MissingFile_Throws() =>
            Assert.ThrowsAny<Exception>(() => EventFileArchive.ListEventFiles(Path.Combine(_tempRoot, "nope.zip")));

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
