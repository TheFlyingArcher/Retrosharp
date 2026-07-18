using System.Globalization;

using CsvHelper;

using Retrosharp.Format;

namespace Retrosharp.Format.Tests
{
    public class BioFileMappingTests
    {
        private static async Task<List<BioFile>> ParseFixtureAsync()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "biofile_sample.csv");
            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            csv.Context.RegisterClassMap<BioFileMapping>();
            return await csv.GetRecordsAsync<BioFile>().ToListAsync();
        }

        [Fact]
        public async Task Parse_FullyPopulatedRow_MapsEveryFieldToTheCorrectColumn()
        {
            var records = await ParseFixtureAsync();
            var aaron = records.Single(r => r.RetrosheetId == "aaroh101");

            Assert.Equal("Aaron", aaron.LastName);
            Assert.Equal("Hank", aaron.UseName);
            Assert.Equal("Henry Louis Aaron", aaron.FullName);
            Assert.Equal(new DateTime(1934, 2, 5), aaron.BirthDate);
            Assert.Equal(new DateTime(2021, 1, 22), aaron.DeathDate);
            Assert.Equal(new DateTime(1954, 4, 13), aaron.PlayerDebut);
            Assert.Equal(new DateTime(1976, 10, 3), aaron.PlayerFinalGame);
            Assert.Equal('R', aaron.Bats);
            Assert.Equal('R', aaron.Throws);
            Assert.True(aaron.InHallOfFame);
        }

        [Fact]
        public async Task Parse_NonHofPlayer_InHallOfFameIsFalse()
        {
            var records = await ParseFixtureAsync();
            var tommie = records.Single(r => r.RetrosheetId == "aarot101");

            Assert.False(tommie.InHallOfFame);
        }

        [Fact]
        public async Task Parse_ManagerAndUmpireDates_MapToTheCorrectColumns()
        {
            // Guards against the exact class of bug previously present in this mapping: a
            // manager/umpire debut or final-game date silently landing in the wrong column.
            var records = await ParseFixtureAsync();
            var addy = records.Single(r => r.RetrosheetId == "addyb101");

            Assert.Equal(new DateTime(1875, 10, 4), addy.ManagerDebut);
            Assert.Equal(new DateTime(1877, 8, 21), addy.FinalManagerGame);
            Assert.Equal(new DateTime(1875, 10, 28), addy.UmpireDebut);
            Assert.Equal(new DateTime(1875, 10, 28), addy.FinalUmpireGame);
        }

        [Fact]
        public async Task Parse_DayZeroBirthdate_NormalizesToFirstOfMonth()
        {
            var records = await ParseFixtureAsync();
            var addy = records.Single(r => r.RetrosheetId == "addyb101");

            Assert.Equal(new DateTime(1842, 2, 1), addy.BirthDate);
        }

        [Fact]
        public async Task Parse_MonthAndDayZeroBirthdate_NormalizesToJanuaryFirst()
        {
            var records = await ParseFixtureAsync();
            var abreu = records.Single(r => r.RetrosheetId == "abree101");

            Assert.Equal(new DateTime(1901, 1, 1), abreu.BirthDate);
        }

        [Fact]
        public async Task Parse_BlankUseNameAndBirthdate_MapsToNull()
        {
            // A blank optional field means no value was recorded, not a value of "" -- prefer
            // null so the database doesn't waste space with empty strings for the large
            // fraction of biographical fields that are routinely unpopulated.
            var records = await ParseFixtureAsync();
            var aberdino = records.Single(r => r.RetrosheetId == "aberu101");

            Assert.Null(aberdino.UseName);
            Assert.Null(aberdino.BirthDate);
            Assert.Null(aberdino.BirthCity);
        }

        [Fact]
        public async Task Parse_BlankDeathAndCemeteryFields_MapToNull()
        {
            // Eufemio Abreu has no recorded death, so every death/cemetery field is blank.
            var records = await ParseFixtureAsync();
            var abreu = records.Single(r => r.RetrosheetId == "abree101");

            Assert.Null(abreu.DeathCity);
            Assert.Null(abreu.DeathState);
            Assert.Null(abreu.DeathCountry);
            Assert.Null(abreu.CemetaryName);
            Assert.Null(abreu.BirthName);
            Assert.Null(abreu.AlternateName);
        }
    }
}
