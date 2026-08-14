using Retrosharp.Contract.Game;
using Retrosharp.Format.Standings;

namespace Retrosharp.Format.Tests
{
    /// <summary>
    /// Exercises <see cref="StandingsResolver"/> against hand-built <see cref="Game"/> lists,
    /// mirroring <see cref="GameStatisticsResolverTests"/>' style. See spec/api.md and
    /// spec/frontend-prototype.md's "Resolved: Standings Derivation" note.
    /// </summary>
    public class StandingsResolverTests
    {
        private const short SeasonYear = 2025;

        private static Game Game(int homeFranchiseId, int visitorFranchiseId, byte homeRuns, byte visitorRuns) =>
            new()
            {
                HomeFranchiseId = homeFranchiseId,
                VisitorFranchiseId = visitorFranchiseId,
                HomeTeamRuns = homeRuns,
                VisitorRuns = visitorRuns
            };

        [Fact]
        public void Resolve_TalliesWinsAndLossesFromGameScores()
        {
            var games = new[]
            {
                Game(homeFranchiseId: 1, visitorFranchiseId: 2, homeRuns: 5, visitorRuns: 3), // 1 beats 2
                Game(homeFranchiseId: 2, visitorFranchiseId: 1, homeRuns: 1, visitorRuns: 7), // 1 beats 2
                Game(homeFranchiseId: 1, visitorFranchiseId: 2, homeRuns: 2, visitorRuns: 4)  // 2 beats 1
            };
            var context = new Dictionary<int, (int? LeagueId, string? DivisionCode)>
            {
                [1] = (10, null),
                [2] = (10, null)
            };

            var standings = StandingsResolver.Resolve(SeasonYear, games, context);

            var team1 = standings.Single(s => s.FranchiseId == 1);
            Assert.Equal(2, team1.Wins);
            Assert.Equal(1, team1.Losses);
            Assert.Equal(0, team1.Ties);

            var team2 = standings.Single(s => s.FranchiseId == 2);
            Assert.Equal(1, team2.Wins);
            Assert.Equal(2, team2.Losses);
        }

        [Fact]
        public void Resolve_EqualScoreCountsAsTieForBothTeams_NotWinOrLoss()
        {
            var games = new[] { Game(homeFranchiseId: 1, visitorFranchiseId: 2, homeRuns: 4, visitorRuns: 4) };
            var context = new Dictionary<int, (int? LeagueId, string? DivisionCode)>
            {
                [1] = (10, null),
                [2] = (10, null)
            };

            var standings = StandingsResolver.Resolve(SeasonYear, games, context);

            var team1 = standings.Single(s => s.FranchiseId == 1);
            Assert.Equal(0, team1.Wins);
            Assert.Equal(0, team1.Losses);
            Assert.Equal(1, team1.Ties);
            // A tie doesn't count toward the win-percentage denominator (standard convention).
            Assert.Equal(0f, team1.WinPercentage);
        }

        [Fact]
        public void Resolve_LeagueWideRanking_LeaderGetsRankOneAndLeagueBestRecord()
        {
            var games = new[]
            {
                Game(homeFranchiseId: 1, visitorFranchiseId: 2, homeRuns: 5, visitorRuns: 3),
                Game(homeFranchiseId: 1, visitorFranchiseId: 2, homeRuns: 5, visitorRuns: 3),
                Game(homeFranchiseId: 1, visitorFranchiseId: 2, homeRuns: 1, visitorRuns: 4)
            };
            // No DivisionCode -- pre-divisional-era style, ranked league-wide only.
            var context = new Dictionary<int, (int? LeagueId, string? DivisionCode)>
            {
                [1] = (10, null),
                [2] = (10, null)
            };

            var standings = StandingsResolver.Resolve(SeasonYear, games, context);

            var leader = standings.Single(s => s.FranchiseId == 1);
            Assert.Equal(1, leader.Rank);
            Assert.Equal(0m, leader.GamesBehind);
            Assert.True(leader.LeagueBestRecord);
            Assert.False(leader.DivisionChampion);

            var trailer = standings.Single(s => s.FranchiseId == 2);
            Assert.Equal(2, trailer.Rank);
            Assert.Equal(1.0m, trailer.GamesBehind); // 2-1 vs 1-2 -- one game back
            Assert.False(trailer.LeagueBestRecord);
        }

        [Fact]
        public void Resolve_DivisionRanking_IsIndependentOfLeagueWideRanking()
        {
            // League 10 has two divisions, "E" and "W". Franchise 1 (division E) has the best
            // *division* record but not the best *league-wide* record -- franchise 3 (division
            // W) has more wins overall. DivisionChampion and LeagueBestRecord must diverge.
            var games = new[]
            {
                Game(homeFranchiseId: 1, visitorFranchiseId: 2, homeRuns: 5, visitorRuns: 1), // 1 beats 2
                Game(homeFranchiseId: 1, visitorFranchiseId: 2, homeRuns: 5, visitorRuns: 1), // 1 beats 2
                Game(homeFranchiseId: 3, visitorFranchiseId: 4, homeRuns: 5, visitorRuns: 1), // 3 beats 4
                Game(homeFranchiseId: 3, visitorFranchiseId: 4, homeRuns: 5, visitorRuns: 1), // 3 beats 4
                Game(homeFranchiseId: 3, visitorFranchiseId: 4, homeRuns: 5, visitorRuns: 1)  // 3 beats 4
            };
            var context = new Dictionary<int, (int? LeagueId, string? DivisionCode)>
            {
                [1] = (10, "E"),
                [2] = (10, "E"),
                [3] = (10, "W"),
                [4] = (10, "W")
            };

            var standings = StandingsResolver.Resolve(SeasonYear, games, context);

            var franchise1 = standings.Single(s => s.FranchiseId == 1);
            Assert.True(franchise1.DivisionChampion); // best in division E (2-0)
            Assert.False(franchise1.LeagueBestRecord); // franchise 3 has a better overall record (3-0)
            Assert.Equal(1, franchise1.Rank); // rank reflects the division grouping, not the league
            Assert.Equal(0m, franchise1.GamesBehind); // leads its own division

            var franchise3 = standings.Single(s => s.FranchiseId == 3);
            Assert.True(franchise3.DivisionChampion); // best in division W (3-0)
            Assert.True(franchise3.LeagueBestRecord); // best overall record in league 10
        }

        [Fact]
        public void Resolve_FranchiseWithNoLeagueId_IsExcludedFromResult()
        {
            var games = new[] { Game(homeFranchiseId: 1, visitorFranchiseId: 2, homeRuns: 5, visitorRuns: 3) };
            var context = new Dictionary<int, (int? LeagueId, string? DivisionCode)>
            {
                [1] = (10, null),
                [2] = (null, null) // e.g. a franchise not found, or genuinely leagueless data
            };

            var standings = StandingsResolver.Resolve(SeasonYear, games, context);

            Assert.Single(standings);
            Assert.Equal(1, standings[0].FranchiseId);
        }

        [Fact]
        public void Resolve_SeasonYear_IsStampedOnEveryEntry()
        {
            var games = new[] { Game(homeFranchiseId: 1, visitorFranchiseId: 2, homeRuns: 5, visitorRuns: 3) };
            var context = new Dictionary<int, (int? LeagueId, string? DivisionCode)>
            {
                [1] = (10, null),
                [2] = (10, null)
            };

            var standings = StandingsResolver.Resolve(SeasonYear, games, context);

            Assert.All(standings, s => Assert.Equal(SeasonYear, s.SeasonYear));
        }
    }
}
