using System.IO.Compression;

using Retrosharp.Engine.Console.Saga;

namespace Retrosharp.Engine.Console.Tests
{
    /// <summary>
    /// Tests for <see cref="BioFileArchive"/> (Step 11f, see spec/retrosheet-auto-download.md).
    /// </summary>
    public sealed class BioFileArchiveTests : IDisposable
    {
        private readonly string _tempRoot;

        public BioFileArchiveTests()
        {
            _tempRoot = Path.Combine(Path.GetTempPath(), "retrosharp-biofile-archive-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempRoot);
        }

        public void Dispose()
        {
            try { if (Directory.Exists(_tempRoot)) Directory.Delete(_tempRoot, recursive: true); }
            catch { /* best effort */ }
        }

        [Theory]
        [InlineData("biofile0.csv", true)]
        [InlineData("BIOFILE0.CSV", true)]
        [InlineData("biofile.csv", false)]   // legacy 33-column format -- not what the parser reads
        [InlineData("umpires0.csv", false)]
        [InlineData("biofile0.txt", false)]
        [InlineData("biofile00.csv", false)]
        [InlineData("", false)]
        public void IsBioFile(string name, bool expected) =>
            Assert.Equal(expected, BioFileArchive.IsBioFile(name));

        [Fact]
        public void Extract_WritesBiofile0_IgnoringTheOtherSevenFiles()
        {
            var zipPath = CreateArchive("biodata.zip",
                ("biofile0.csv", "id,last\naardh101,Aardsma"),
                ("biofile.csv", "legacy"),
                ("umpires0.csv", "umps"),
                ("coaches0.csv", "coaches"));
            var target = Path.Combine(_tempRoot, "out");

            var path = BioFileArchive.Extract(zipPath, target);

            Assert.Equal(Path.Combine(target, "biofile0.csv"), path);
            Assert.Equal("id,last\naardh101,Aardsma", File.ReadAllText(path));
            Assert.False(File.Exists(Path.Combine(target, "biofile.csv")));
            Assert.False(File.Exists(Path.Combine(target, "umpires0.csv")));
        }

        [Fact]
        public void Extract_FlattensNestedEntry()
        {
            var zipPath = CreateArchive("biodata.zip", ("nested/biofile0.csv", "rows"));
            var target = Path.Combine(_tempRoot, "out");

            var path = BioFileArchive.Extract(zipPath, target);

            Assert.Equal(Path.Combine(target, "biofile0.csv"), path);
            Assert.False(Directory.Exists(Path.Combine(target, "nested")));
        }

        [Fact]
        public void Extract_OverwritesAnExistingFile()
        {
            var target = Path.Combine(_tempRoot, "out");
            Directory.CreateDirectory(target);
            File.WriteAllText(Path.Combine(target, "biofile0.csv"), "stale");
            var zipPath = CreateArchive("biodata.zip", ("biofile0.csv", "fresh"));

            var path = BioFileArchive.Extract(zipPath, target);

            Assert.Equal("fresh", File.ReadAllText(path));
        }

        [Fact]
        public void Extract_NoBiofile0_ThrowsInvalidData()
        {
            var zipPath = CreateArchive("biodata.zip", ("biofile.csv", "legacy only"), ("umpires0.csv", "x"));

            var ex = Assert.Throws<InvalidDataException>(() => BioFileArchive.Extract(zipPath, Path.Combine(_tempRoot, "out")));
            Assert.Contains("no biofile0.csv", ex.Message);
        }

        [Fact]
        public void Extract_MissingArchive_Throws() =>
            Assert.ThrowsAny<Exception>(() => BioFileArchive.Extract(Path.Combine(_tempRoot, "nope.zip"), _tempRoot));

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
