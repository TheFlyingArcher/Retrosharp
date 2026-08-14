using Retrosharp.Contract.Franchise;
using Retrosharp.Contract.Standing;
using Retrosharp.Format.Standings;

namespace Retrosharp.Format.Tests
{
    /// <summary>
    /// Exercises <see cref="FranchiseCareerSummaryResolver"/> against hand-built
    /// <see cref="Franchise"/>/<see cref="FranchiseSeasonStanding"/> fixtures, mirroring
    /// <see cref="StandingsResolverTests"/>' style. See spec/frontend-prototype.md's "Resolved:
    /// Franchise All-Time Summary" note.
    /// </summary>
    public class FranchiseCareerSummaryResolverTests
    {
        private static Franchise Era(int id, string identifier, DateTime start, string city, string nickname, string code = "XXX") =>
            new()
            {
                Id = id,
                FranchiseIdentifier = identifier,
                FranchiseStart = start,
                FranchiseCode = code,
                PlayingCity = city,
                Nickname = nickname
            };

        private static FranchiseSeasonStanding Standing(int franchiseId, short season, short wins, short losses, short ties = 0) =>
            new() { FranchiseId = franchiseId, SeasonYear = season, Wins = wins, Losses = losses, Ties = ties };

        [Fact]
        public void Resolve_MultiEraLineage_CollapsesIntoOneRowRepresentedByMostRecentEra()
        {
            // Montreal Expos (1969-2004) renamed to Washington Nationals (2005-present) --
            // same lineage (FranchiseIdentifier), two Franchise rows.
            var eras = new[]
            {
                Era(1, "MON", new DateTime(1969, 1, 1), "Montreal", "Expos"),
                Era(2, "MON", new DateTime(2005, 1, 1), "Washington", "Nationals")
            };
            var standings = new[]
            {
                Standing(1, 2000, wins: 67, losses: 95),
                Standing(2, 2005, wins: 81, losses: 81)
            };

            var summaries = FranchiseCareerSummaryResolver.Resolve(eras, standings);

            var summary = Assert.Single(summaries);
            Assert.Equal(2, summary.FranchiseId); // represented by the most recent era
            Assert.Equal("Washington Nationals", summary.CurrentName);
            Assert.Equal(new[] { "Montreal Expos" }, summary.FormerNames);
            Assert.Equal(1969, summary.FirstSeasonYear);
            Assert.Equal(148, summary.Wins); // 67 + 81, summed across BOTH eras
            Assert.Equal(176, summary.Losses);
            Assert.Equal(324, summary.GamesPlayed);
        }

        [Fact]
        public void Resolve_SingleEraLineage_HasNoFormerNames()
        {
            var eras = new[] { Era(1, "SDN", new DateTime(1969, 1, 1), "San Diego", "Padres") };
            var standings = new[] { Standing(1, 2025, wins: 90, losses: 72) };

            var summary = Assert.Single(FranchiseCareerSummaryResolver.Resolve(eras, standings));

            Assert.Empty(summary.FormerNames);
            Assert.Equal("San Diego Padres", summary.CurrentName);
        }

        [Fact]
        public void Resolve_CountsSeasonsAboveAndBelowFiveHundred_ExcludingExactlyFiveHundred()
        {
            var eras = new[] { Era(1, "SDN", new DateTime(1969, 1, 1), "San Diego", "Padres") };
            var standings = new[]
            {
                Standing(1, 2021, wins: 90, losses: 72), // above .500
                Standing(1, 2022, wins: 60, losses: 102), // below .500
                Standing(1, 2023, wins: 81, losses: 81) // exactly .500 -- neither
            };

            var summary = Assert.Single(FranchiseCareerSummaryResolver.Resolve(eras, standings));

            Assert.Equal(1, summary.SeasonsAboveFiveHundred);
            Assert.Equal(1, summary.SeasonsBelowFiveHundred);
        }

        [Fact]
        public void Resolve_FranchiseWithNoStandingsYet_ReturnsZeroedRowNotError()
        {
            var eras = new[] { Era(1, "SDN", new DateTime(1969, 1, 1), "San Diego", "Padres") };

            var summary = Assert.Single(FranchiseCareerSummaryResolver.Resolve(eras, []));

            Assert.Equal(0, summary.GamesPlayed);
            Assert.Equal(0, summary.Wins);
            Assert.Equal(0f, summary.WinPercentage);
        }

        [Fact]
        public void Resolve_MultipleLineages_EachGetsItsOwnRow()
        {
            var eras = new[]
            {
                Era(1, "SDN", new DateTime(1969, 1, 1), "San Diego", "Padres"),
                Era(2, "SEA", new DateTime(1977, 1, 1), "Seattle", "Mariners")
            };
            var standings = new[]
            {
                Standing(1, 2025, wins: 90, losses: 72),
                Standing(2, 2025, wins: 85, losses: 77)
            };

            var summaries = FranchiseCareerSummaryResolver.Resolve(eras, standings);

            Assert.Equal(2, summaries.Count);
            Assert.Equal(90, summaries.Single(s => s.CurrentName == "San Diego Padres").Wins);
            Assert.Equal(85, summaries.Single(s => s.CurrentName == "Seattle Mariners").Wins);
        }
    }
}
