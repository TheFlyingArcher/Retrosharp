using Retrosharp.Contract.Batting;
using Retrosharp.Contract.Fielding;
using Retrosharp.Contract.GameEvent;
using Retrosharp.Contract.Pitching;
using Retrosharp.Format.PlayByPlay;

namespace Retrosharp.Format.Tests
{
    /// <summary>
    /// Exercises <see cref="GameReconciliationResolver"/> against hand-built
    /// <see cref="GameEventPlayRecord"/>/<see cref="GameStatisticsDelta"/> graphs, mirroring
    /// <see cref="GameStatisticsResolverTests"/>' style. See spec/phase-1-build-plan.md Step 6e.
    /// </summary>
    public class GameReconciliationResolverTests
    {
        private const int HomeFranchiseId = 1;
        private const int VisitorFranchiseId = 2;
        private const short SeasonYear = 2025;

        private static GameEventPlayRecord Play(GameEvent evt, params GameEventRunnerRecord[] runners) =>
            new() { Event = evt, Runners = runners };

        private static GameEventRunnerRecord Runner(
            int personId, BaseState startBase, BaseState endBase, bool isOut = false,
            bool isRBI = false, bool isEarnedRun = false, int? responsiblePitcherId = null) =>
            new()
            {
                Runner = new GameEventRunner
                {
                    PersonId = personId,
                    StartBase = startBase,
                    EndBase = endBase,
                    IsOut = isOut,
                    IsRBI = isRBI,
                    IsEarnedRun = isEarnedRun,
                    ResponsiblePitcherId = responsiblePitcherId
                },
                FieldingCredits = []
            };

        [Fact]
        public void ResolveTeamTotals_Batting_AggregatesAcrossPlayersAndComputesRbiFromPlays()
        {
            var statistics = new GameStatisticsDelta
            {
                Battings =
                [
                    new BattingDelta { PersonId = 10, FranchiseId = VisitorFranchiseId, SeasonYear = SeasonYear, PlateAppearances = 1, AtBats = 1, Hits = 1 },
                    new BattingDelta { PersonId = 11, FranchiseId = VisitorFranchiseId, SeasonYear = SeasonYear, PlateAppearances = 1, AtBats = 1, Hits = 1 }
                ],
                Pitchings = [],
                Fieldings = []
            };

            // Batter 10's single scores runner 9 from third -- an RBI not tracked anywhere in
            // BattingDelta, so ResolveTeamTotals must derive it fresh from the play itself.
            var evt = new GameEvent { TeamAtBat = "V", BatterId = 10, PitcherId = 20, EventType = GameEventType.Single };
            var play = Play(
                evt,
                Runner(10, BaseState.BattersBox, BaseState.First),
                Runner(9, BaseState.Third, BaseState.Home, isRBI: true));

            var totals = GameReconciliationResolver.ResolveTeamTotals(HomeFranchiseId, VisitorFranchiseId, [play], statistics);

            var visitorBatting = Assert.Single(totals.Battings, b => b.FranchiseId == VisitorFranchiseId);
            Assert.Equal(2, visitorBatting.PlateAppearances);
            Assert.Equal(2, visitorBatting.Hits);
            Assert.Equal(1, visitorBatting.RunsBattedIn);
        }

        [Fact]
        public void ResolveTeamTotals_Pitching_CountsDistinctPitchersAndSumsEarnedRunsWildPitchesBalks()
        {
            var statistics = new GameStatisticsDelta
            {
                Battings = [],
                Pitchings =
                [
                    new PitchingDelta { PersonId = 20, FranchiseId = HomeFranchiseId, SeasonYear = SeasonYear, EarnedRuns = 2, WildPitches = 1, Balks = 0 },
                    new PitchingDelta { PersonId = 21, FranchiseId = HomeFranchiseId, SeasonYear = SeasonYear, EarnedRuns = 1, WildPitches = 0, Balks = 1 }
                ],
                Fieldings = []
            };

            var totals = GameReconciliationResolver.ResolveTeamTotals(HomeFranchiseId, VisitorFranchiseId, [], statistics);

            var homePitching = Assert.Single(totals.Pitchings, p => p.FranchiseId == HomeFranchiseId);
            Assert.Equal(2, homePitching.PitchersUsed);
            Assert.Equal(3, homePitching.IndividualEarnedRuns);
            Assert.Equal(1, homePitching.WildPitches);
            Assert.Equal(1, homePitching.Balks);
        }

        [Fact]
        public void ResolveTeamTotals_Fielding_SumsPutoutsAssistsErrorsAcrossPositions()
        {
            var statistics = new GameStatisticsDelta
            {
                Battings = [],
                Pitchings = [],
                Fieldings =
                [
                    new FieldingDelta { PersonId = 60, FranchiseId = VisitorFranchiseId, SeasonYear = SeasonYear, Position = 6, Putouts = 1, Assists = 2 },
                    new FieldingDelta { PersonId = 63, FranchiseId = VisitorFranchiseId, SeasonYear = SeasonYear, Position = 3, Putouts = 3, Errors = 1 }
                ]
            };

            var totals = GameReconciliationResolver.ResolveTeamTotals(HomeFranchiseId, VisitorFranchiseId, [], statistics);

            var visitorFielding = Assert.Single(totals.Fieldings, f => f.FranchiseId == VisitorFranchiseId);
            Assert.Equal(4, visitorFielding.Putouts);
            Assert.Equal(2, visitorFielding.Assists);
            Assert.Equal(1, visitorFielding.Errors);
        }

        [Fact]
        public void ResolveIndependentEarnedRuns_CreditsResponsiblePitcher_AndIgnoresUnearnedRuns()
        {
            // Runner 6 (put on by pitcher 30, an earlier at-bat not modeled here) scores an
            // earned run under the CURRENT pitcher (31) -- the inherited-runner case, mirroring
            // GameStatisticsResolverTests' Runs-attribution test. Runner 7 also scores on the
            // same play but is flagged unearned, and must not be counted at all.
            var evt = new GameEvent { TeamAtBat = "V", BatterId = 14, PitcherId = 31, EventType = GameEventType.Single };
            var play = Play(
                evt,
                Runner(14, BaseState.BattersBox, BaseState.First),
                Runner(6, BaseState.Third, BaseState.Home, isRBI: true, isEarnedRun: true, responsiblePitcherId: 30),
                Runner(7, BaseState.Second, BaseState.Home, isRBI: true, isEarnedRun: false, responsiblePitcherId: 31));

            var earnedRuns = GameReconciliationResolver.ResolveIndependentEarnedRuns([play]);

            Assert.Equal(1, earnedRuns[30]);
            Assert.False(earnedRuns.ContainsKey(31));
        }
    }
}
