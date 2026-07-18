using System.Globalization;

using CsvHelper;
using CsvHelper.Configuration;

using Retrosharp.Format;

namespace Retrosharp.Format.Tests
{
    public class GameLogMappingTests
    {
        private static async Task<List<GameLog>> ParseFixtureAsync()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "gamelog_sample.csv");
            using var reader = new StreamReader(path);

            // Real game log files have no header row -- matches the HasHeaderRecord=false
            // configuration in the production GameLogFileService.
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = false };
            using var csv = new CsvReader(reader, config);

            csv.Context.RegisterClassMap<GameLogMap>();
            return await csv.GetRecordsAsync<GameLog>().ToListAsync();
        }

        private static GameLog Find(List<GameLog> records, string visitor, string home, DateTime date, char gameNumber = '0')
        {
            return records.Single(r =>
                r.VisitorTeamCode == visitor &&
                r.HomeTeamCode == home &&
                r.GameDate == date &&
                r.GameNumber == gameNumber);
        }

        [Fact]
        public async Task Parse_HomeLineup_MapsEachBatterToTheCorrectFields()
        {
            // Guards against the off-by-one that previously shifted the entire Home Lineup
            // block by one CSV field, scrambling every home batter's id/name/position.
            var records = await ParseFixtureAsync();
            var game = Find(records, "LAN", "CHN", new DateTime(2025, 3, 18));

            var leadoff = game.HomeStartingLineup.LeadoffBatter;
            Assert.Equal("happi001", leadoff.PlayerId);
            Assert.Equal("Ian Happ", leadoff.PlayerName);
            Assert.Equal("7", leadoff.PlayerPosition);

            var ninth = game.HomeStartingLineup.NinthBatter;
            Assert.Equal("bertj001", ninth.PlayerId);
            Assert.Equal("Jon Berti", ninth.PlayerName);
            Assert.Equal("4", ninth.PlayerPosition);
        }

        [Fact]
        public async Task Parse_VisitorLineup_StillMapsCorrectly()
        {
            // The Visitor Lineup mapping was already correct; confirm the Home Lineup fix
            // didn't disturb it.
            var records = await ParseFixtureAsync();
            var game = Find(records, "LAN", "CHN", new DateTime(2025, 3, 18));

            var leadoff = game.VisitorStartingLineup.LeadoffBatter;
            Assert.Equal("ohtas001", leadoff.PlayerId);
            Assert.Equal("Shohei Ohtani", leadoff.PlayerName);
            Assert.Equal("10", leadoff.PlayerPosition);
        }

        [Fact]
        public async Task Parse_GameDate_ParsesTheYyyyMmDdString()
        {
            var records = await ParseFixtureAsync();
            var game = Find(records, "LAN", "CHN", new DateTime(2025, 3, 18));

            Assert.Equal(new DateTime(2025, 3, 18), game.GameDate);
        }

        [Fact]
        public async Task Parse_BlankAdditionalInformation_MapsToEmptyString()
        {
            var records = await ParseFixtureAsync();
            var game = Find(records, "LAN", "CHN", new DateTime(2025, 3, 18));

            Assert.Equal(string.Empty, game.AdditionalInformation);
            Assert.Equal('Y', game.AcquisitionInfo);
        }

        [Fact]
        public async Task Parse_PopulatedAdditionalInformation_MapsToTheCorrectColumn()
        {
            var records = await ParseFixtureAsync();
            var game = Find(records, "NYN", "MIN", new DateTime(2025, 4, 16));

            Assert.Equal("umpchange,7,ump1b,hamaa901,7,ump2b,(None)", game.AdditionalInformation);
            Assert.Equal('Y', game.AcquisitionInfo);
        }

        [Fact]
        public async Task Parse_Doubleheader_ProducesTwoDistinctGamesByGameNumber()
        {
            var records = await ParseFixtureAsync();

            var game1 = Find(records, "CLE", "MIN", new DateTime(2025, 9, 20), '1');
            var game2 = Find(records, "CLE", "MIN", new DateTime(2025, 9, 20), '2');

            Assert.Equal('1', game1.GameNumber);
            Assert.Equal('2', game2.GameNumber);
            Assert.NotEqual(game1.VisitorScore, game2.VisitorScore);
        }

        [Fact]
        public async Task Parse_BlankGameAttendance_MapsToNull()
        {
            // A real suspended-and-completed-later game with no recorded attendance figure.
            // GameAttendance was previously a non-nullable int, which threw a
            // TypeConverterException on this exact row.
            var records = await ParseFixtureAsync();
            var game = Find(records, "CLE", "MIN", new DateTime(2025, 5, 19));

            Assert.Null(game.GameAttendance);
        }
    }
}
