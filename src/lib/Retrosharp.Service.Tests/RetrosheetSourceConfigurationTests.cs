using Retrosharp.Configuration;

namespace Retrosharp.Service.Tests
{
    /// <summary>
    /// Pure tests for <see cref="RetrosheetSourceConfiguration"/>'s URL composition and
    /// working-root fallback (Step 11a, see spec/retrosheet-auto-download.md).
    /// </summary>
    public class RetrosheetSourceConfigurationTests
    {
        [Theory]
        [InlineData(2024, "https://www.retrosheet.org/gamelogs/gl2024.zip")]
        [InlineData(1998, "https://www.retrosheet.org/gamelogs/gl1998.zip")]
        public void GameLogUri_SubstitutesSeasonIntoDefaultTemplate(int season, string expected)
        {
            var config = new RetrosheetSourceConfiguration();

            Assert.Equal(expected, config.GameLogUri(season).ToString());
        }

        [Theory]
        [InlineData(2024, "https://www.retrosheet.org/events/2024eve.zip")]
        [InlineData(1871, "https://www.retrosheet.org/events/1871eve.zip")]
        public void EventArchiveUri_SubstitutesSeasonIntoDefaultTemplate(int season, string expected)
        {
            var config = new RetrosheetSourceConfiguration();

            Assert.Equal(expected, config.EventArchiveUri(season).ToString());
        }

        [Fact]
        public void BioFileUri_HasNoSeasonComponent()
        {
            var config = new RetrosheetSourceConfiguration();

            Assert.Equal("https://www.retrosheet.org/downloads/biodata.zip", config.BioFileUri().ToString());
        }

        [Fact]
        public void Uris_HonourOverriddenBaseUrlAndTemplates()
        {
            var config = new RetrosheetSourceConfiguration
            {
                BaseUrl = "http://localhost:9000",           // no trailing slash on purpose
                GameLogArchivePath = "mirror/gl/{season}.zip",
                EventArchivePath = "mirror/ev/{season}.zip"
            };

            Assert.Equal("http://localhost:9000/mirror/gl/2023.zip", config.GameLogUri(2023).ToString());
            Assert.Equal("http://localhost:9000/mirror/ev/2023.zip", config.EventArchiveUri(2023).ToString());
        }

        [Fact]
        public void ResolvedWorkingRoot_EmptyFallsBackToTempSubdirectory()
        {
            var config = new RetrosheetSourceConfiguration { WorkingRoot = "   " };

            Assert.Equal(Path.Combine(Path.GetTempPath(), "retrosharp-import"), config.ResolvedWorkingRoot);
        }

        [Fact]
        public void ResolvedWorkingRoot_HonoursExplicitValue()
        {
            var config = new RetrosheetSourceConfiguration { WorkingRoot = "/data/retrosharp/work" };

            Assert.Equal("/data/retrosharp/work", config.ResolvedWorkingRoot);
        }
    }
}
