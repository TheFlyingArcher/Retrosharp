using Retrosharp.Format;

namespace Retrosharp.Format.Tests
{
    public class RetrosheetDateParserTests
    {
        [Fact]
        public void Parse_FullDate_ReturnsExactDate()
        {
            var result = RetrosheetDateParser.Parse("19340205");

            Assert.Equal(new DateTime(1934, 2, 5), result);
        }

        [Fact]
        public void Parse_MonthAndDayBothZero_NormalizesToJanuaryFirst()
        {
            // Real biofile0.csv row (abree101): year known, month and day both unknown.
            var result = RetrosheetDateParser.Parse("19010000");

            Assert.Equal(new DateTime(1901, 1, 1), result);
        }

        [Fact]
        public void Parse_DayZero_NormalizesToFirstOfMonth()
        {
            // Real biofile0.csv row (addyb101): month known, day unknown.
            var result = RetrosheetDateParser.Parse("18420200");

            Assert.Equal(new DateTime(1842, 2, 1), result);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Parse_MissingValue_ReturnsNull(string? raw)
        {
            var result = RetrosheetDateParser.Parse(raw);

            Assert.Null(result);
        }

        [Fact]
        public void Parse_UnparseableValue_ReturnsNull()
        {
            var result = RetrosheetDateParser.Parse("not-a-date");

            Assert.Null(result);
        }
    }
}
