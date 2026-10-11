using Retrosharp.Contract.Franchise;
using Retrosharp.Contract.Standing;
using Retrosharp.Format.Standings;

namespace Retrosharp.Format.Tests
{
    /// <summary>
    /// Exercises <see cref="FormerFranchiseNameResolver"/> against hand-built
    /// <see cref="Franchise"/>/<see cref="FranchiseSeasonStanding"/> fixtures. See
    /// spec/former-franchise-names.md.
    /// </summary>
    public class FormerFranchiseNameResolverTests
    {
        private static Franchise Era(
            int id,
            string identifier,
            int startYear,
            int? endYear,
            string location,
            string nickname,
            string code = "XXX",
            string? division = null) =>
            new()
            {
                Id = id,
                FranchiseIdentifier = identifier,
                FranchiseStart = new DateTime(startYear, 1, 1),
                FranchiseEnd = endYear is null ? null : new DateTime(endYear.Value, 12, 31),
                FranchiseCode = code,
                FranchiseLocation = location,
                Nickname = nickname,
                DivisionCode = division
            };

        private static FranchiseSeasonStanding Standing(int franchiseId, short season, short wins, short losses, short ties = 0) =>
            new() { FranchiseId = franchiseId, SeasonYear = season, Wins = wins, Losses = losses, Ties = ties };

        [Fact]
        public void Resolve_RenamedLineage_ListsOldNameWithNextNameAndRecord()
        {
            var eras = new[]
            {
                Era(1, "MON", 1969, 2004, "Montreal", "Expos", "MON"),
                Era(2, "MON", 2005, null, "Washington", "Nationals", "WAS")
            };
            var standings = new[]
            {
                Standing(1, 2000, 67, 95),
                Standing(1, 2001, 68, 94),
                Standing(2, 2005, 81, 81)
            };

            var names = FormerFranchiseNameResolver.Resolve(eras, standings);

            var expos = Assert.Single(names);
            Assert.Equal("Montreal Expos", expos.Name);
            Assert.Equal("MON", expos.FranchiseCode);
            Assert.Equal(1, expos.FranchiseId);
            Assert.Equal("Washington Nationals", expos.NextName);
            Assert.Equal(2, expos.NextFranchiseId);
            Assert.Equal(1969, expos.FirstSeasonYear);
            Assert.Equal(2004, expos.LastSeasonYear);
            Assert.Equal(135, expos.Wins); // current era's 81 wins are not included
            Assert.Equal(189, expos.Losses);
            Assert.Equal(324, expos.GamesPlayed);
        }

        [Fact]
        public void Resolve_MultiNameLineage_EachNameBecomesItsImmediateSuccessor()
        {
            var eras = new[]
            {
                Era(1, "ANA", 1965, 1996, "California", "Angels"),
                Era(2, "ANA", 1997, 2004, "Anaheim", "Angels"),
                Era(3, "ANA", 2005, 2015, "Los Angeles", "Angels of Anaheim"),
                Era(4, "ANA", 2016, null, "Los Angeles", "Angels")
            };

            var names = FormerFranchiseNameResolver.Resolve(eras, []);

            Assert.Equal(3, names.Count);
            Assert.Equal("Anaheim Angels", names.Single(n => n.Name == "California Angels").NextName);
            Assert.Equal(2, names.Single(n => n.Name == "California Angels").NextFranchiseId);
            Assert.Equal("Los Angeles Angels of Anaheim", names.Single(n => n.Name == "Anaheim Angels").NextName);
            Assert.Equal("Los Angeles Angels", names.Single(n => n.Name == "Los Angeles Angels of Anaheim").NextName);
        }

        [Fact]
        public void Resolve_LineageThatNeverChangedNames_YieldsNoRows()
        {
            var eras = new[] { Era(1, "SDN", 1969, null, "San Diego", "Padres") };

            Assert.Empty(FormerFranchiseNameResolver.Resolve(eras, []));
        }

        [Fact]
        public void Resolve_SameCityRename_IsListed()
        {
            var eras = new[]
            {
                Era(1, "CLE", 1915, 2021, "Cleveland", "Indians"),
                Era(2, "CLE", 2022, null, "Cleveland", "Guardians")
            };

            var names = FormerFranchiseNameResolver.Resolve(eras, []);

            Assert.Equal("Cleveland Indians", Assert.Single(names).Name);
        }

        [Fact]
        public void Resolve_SameNicknameMove_IsListed()
        {
            var eras = new[]
            {
                Era(1, "OAK", 1901, 1954, "Philadelphia", "Athletics"),
                Era(2, "OAK", 1955, 1967, "Kansas City", "Athletics"),
                Era(3, "OAK", 1968, null, "Oakland", "Athletics")
            };

            var names = FormerFranchiseNameResolver.Resolve(eras, []);

            Assert.Equal(
                new[] { "Kansas City Athletics", "Philadelphia Athletics" },
                names.Select(n => n.Name).OrderBy(n => n).ToArray());
        }

        [Fact]
        public void Resolve_UsesFranchiseLocationNotPlayingCity()
        {
            var era = Era(1, "TBA", 1998, 2007, "Tampa Bay", "Devil Rays");
            era.PlayingCity = "St. Petersburg";
            var eras = new[] { era, Era(2, "TBA", 2008, null, "Tampa Bay", "Rays") };

            var names = FormerFranchiseNameResolver.Resolve(eras, []);

            Assert.Equal("Tampa Bay Devil Rays", Assert.Single(names).Name);
            Assert.Equal("Tampa Bay Rays", names[0].NextName);
        }

        [Fact]
        public void Resolve_NameRecurringAfterGap_CollapsesToOneRowWithSummedRecord()
        {
            // Boston Braves 1912-35 and 1941-52 around the Bees years.
            var eras = new[]
            {
                Era(1, "ATL", 1912, 1935, "Boston", "Braves"),
                Era(2, "ATL", 1936, 1940, "Boston", "Bees"),
                Era(3, "ATL", 1941, 1952, "Boston", "Braves"),
                Era(4, "ATL", 1953, 1965, "Milwaukee", "Braves"),
                Era(5, "ATL", 1966, null, "Atlanta", "Braves")
            };
            var standings = new[]
            {
                Standing(1, 1920, 62, 90),
                Standing(2, 1937, 79, 73),
                Standing(3, 1950, 83, 71)
            };

            var names = FormerFranchiseNameResolver.Resolve(eras, standings);

            Assert.Equal(3, names.Count);
            var braves = Assert.Single(names, n => n.Name == "Boston Braves");
            Assert.Equal(1912, braves.FirstSeasonYear);
            Assert.Equal(1952, braves.LastSeasonYear);
            Assert.Equal(3, braves.FranchiseId); // most recent era of this name
            Assert.Equal(145, braves.Wins);
            Assert.Equal(161, braves.Losses);
            Assert.Equal("Milwaukee Braves", braves.NextName); // follows the last Boston Braves era
            Assert.Equal(4, braves.NextFranchiseId);
            Assert.Equal("Boston Braves", Assert.Single(names, n => n.Name == "Boston Bees").NextName);
        }

        [Fact]
        public void Resolve_ErasDifferingOnlyByDivisionOrCode_ShareOneName()
        {
            var eras = new[]
            {
                Era(1, "ANA", 1961, 1964, "Los Angeles", "Angels", "LAA", "AL"),
                Era(2, "ANA", 1965, 1996, "California", "Angels", "CAL", "AL"),
                Era(3, "ANA", 1997, 2004, "California", "Angels", "ANA", "AL"),
                Era(4, "ANA", 2005, null, "Los Angeles", "Angels", "ANA", "AL")
            };
            var standings = new[]
            {
                Standing(2, 1980, 65, 95),
                Standing(3, 2000, 82, 80)
            };

            var names = FormerFranchiseNameResolver.Resolve(eras, standings);

            var california = Assert.Single(names);
            Assert.Equal("California Angels", california.Name);
            Assert.Equal(1965, california.FirstSeasonYear);
            Assert.Equal(2004, california.LastSeasonYear);
            Assert.Equal(147, california.Wins);
            Assert.DoesNotContain(names, n => n.Name == "Los Angeles Angels");
        }

        [Fact]
        public void Resolve_SameNameInDifferentLineages_StaysSeparate()
        {
            // Both Washington Senators clubs, in different lineages.
            var eras = new[]
            {
                Era(1, "MIN", 1901, 1960, "Washington", "Senators"),
                Era(2, "MIN", 1961, null, "Minnesota", "Twins"),
                Era(3, "TEX", 1961, 1971, "Washington", "Senators"),
                Era(4, "TEX", 1972, null, "Texas", "Rangers")
            };

            var names = FormerFranchiseNameResolver.Resolve(eras, []);

            Assert.Equal(2, names.Count);
            Assert.Equal("Minnesota Twins", names.Single(n => n.FranchiseId == 1).NextName);
            Assert.Equal("Texas Rangers", names.Single(n => n.FranchiseId == 3).NextName);
        }

        [Fact]
        public void Resolve_NameWithNoStandings_HasZeroRecord()
        {
            var eras = new[]
            {
                Era(1, "MON", 1969, 2004, "Montreal", "Expos"),
                Era(2, "MON", 2005, null, "Washington", "Nationals")
            };

            var expos = Assert.Single(FormerFranchiseNameResolver.Resolve(eras, []));

            Assert.Equal(0, expos.GamesPlayed);
            Assert.Equal(0f, expos.WinPercentage);
        }
    }
}
