using Retrosharp.Contract.GameEvent;
using Retrosharp.Format.PlayByPlay;

namespace Retrosharp.Format.Tests
{
    /// <summary>
    /// Exercises <see cref="GameStatisticsResolver"/> against hand-built
    /// <see cref="GameEventPlayRecord"/> graphs -- constructed directly, not via
    /// <see cref="GameEventResolver"/>, since these tests target the statistics-aggregation
    /// logic in isolation from the play-parsing pipeline already covered by
    /// <see cref="GameEventResolverTests"/>.
    /// </summary>
    public class GameStatisticsResolverTests
    {
        private const int HomeFranchiseId = 1;
        private const int VisitorFranchiseId = 2;
        private const short SeasonYear = 2025;

        private static GameEventPlayRecord Play(
            GameEvent evt,
            params GameEventRunnerRecord[] runners) =>
            new() { Event = evt, Runners = runners };

        private static GameEventRunnerRecord Runner(
            int personId, BaseState startBase, BaseState endBase, bool isOut = false,
            int? responsiblePitcherId = null, params GameEventFieldingCredit[] credits) =>
            new()
            {
                Runner = new GameEventRunner
                {
                    PersonId = personId,
                    StartBase = startBase,
                    EndBase = endBase,
                    IsOut = isOut,
                    ResponsiblePitcherId = responsiblePitcherId
                },
                FieldingCredits = credits
            };

        private static GameEventFieldingCredit Credit(int personId, FieldingCreditType creditType, int sequence, byte position) =>
            new() { PersonId = personId, CreditType = creditType, Sequence = sequence, Position = position };

        private static GameStatisticsDelta Resolve(params GameEventPlayRecord[] plays) =>
            GameStatisticsResolver.Resolve(HomeFranchiseId, VisitorFranchiseId, SeasonYear, plays, new Dictionary<int, short>());

        [Fact]
        public void Resolve_Single_BatterAndPitcherLinesUpdated()
        {
            var evt = new GameEvent { TeamAtBat = "V", BatterId = 10, PitcherId = 20, EventType = GameEventType.Single };
            var play = Play(evt, Runner(10, BaseState.BattersBox, BaseState.First));

            var delta = Resolve(play);

            var batting = Assert.Single(delta.Battings);
            Assert.Equal(10, batting.PersonId);
            Assert.Equal(VisitorFranchiseId, batting.FranchiseId);
            Assert.Equal(1, batting.PlateAppearances);
            Assert.Equal(1, batting.AtBats);
            Assert.Equal(1, batting.Hits);

            var pitching = Assert.Single(delta.Pitchings);
            Assert.Equal(20, pitching.PersonId);
            Assert.Equal(HomeFranchiseId, pitching.FranchiseId);
            Assert.Equal(1, pitching.Hits);
        }

        [Fact]
        public void Resolve_Walk_CountsPlateAppearanceButNotAtBat()
        {
            var evt = new GameEvent { TeamAtBat = "H", BatterId = 11, PitcherId = 21, EventType = GameEventType.Walk };
            var play = Play(evt, Runner(11, BaseState.BattersBox, BaseState.First));

            var delta = Resolve(play);

            var batting = Assert.Single(delta.Battings);
            Assert.Equal(1, batting.PlateAppearances);
            Assert.Equal(0, batting.AtBats);
            Assert.Equal(1, batting.BaseOnBalls);
            Assert.Equal(0, batting.IntentionalBb);

            Assert.Equal(1, Assert.Single(delta.Pitchings).BaseOnBalls);
        }

        [Fact]
        public void Resolve_IntentionalWalk_CountsBothBaseOnBallsAndIntentionalBb()
        {
            var evt = new GameEvent { TeamAtBat = "H", BatterId = 11, PitcherId = 21, EventType = GameEventType.IntentionalWalk };
            var play = Play(evt, Runner(11, BaseState.BattersBox, BaseState.First));

            var delta = Resolve(play);

            var batting = Assert.Single(delta.Battings);
            Assert.Equal(1, batting.BaseOnBalls);
            Assert.Equal(1, batting.IntentionalBb);
        }

        [Fact]
        public void Resolve_SacrificeFly_ExcludedFromAtBatsButCountsAsPlateAppearance()
        {
            var evt = new GameEvent { TeamAtBat = "V", BatterId = 12, PitcherId = 22, EventType = GameEventType.FlyOut, IsSacFly = true };
            var play = Play(evt, Runner(12, BaseState.BattersBox, BaseState.Home, isOut: true));

            var delta = Resolve(play);

            var batting = Assert.Single(delta.Battings);
            Assert.Equal(1, batting.PlateAppearances);
            Assert.Equal(0, batting.AtBats);
            Assert.Equal(1, batting.SacrificeFlies);
        }

        [Fact]
        public void Resolve_StolenBase_CreditedToRunnerNotCurrentBatter()
        {
            // The runner (5) steals while a different player (13) is at the plate.
            var evt = new GameEvent { TeamAtBat = "V", BatterId = 13, PitcherId = 23, EventType = GameEventType.StolenBase };
            var play = Play(evt, Runner(5, BaseState.First, BaseState.Second));

            var delta = Resolve(play);

            var runnerBatting = Assert.Single(delta.Battings, b => b.PersonId == 5);
            Assert.Equal(1, runnerBatting.StolenBases);

            // The batter still gets a Battings row (PA tracking would apply on their own event,
            // not this one -- StolenBase isn't a PA-ending event) but no PA/AB from this play.
            var batterRow = delta.Battings.SingleOrDefault(b => b.PersonId == 13);
            Assert.NotNull(batterRow);
            Assert.Equal(0, batterRow!.PlateAppearances);
        }

        [Fact]
        public void Resolve_CaughtStealing_CreditedToRunnerAsOut()
        {
            var evt = new GameEvent { TeamAtBat = "V", BatterId = 13, PitcherId = 23, EventType = GameEventType.CaughtStealing };
            var play = Play(evt, Runner(5, BaseState.First, BaseState.Second, isOut: true));

            var delta = Resolve(play);

            var runnerBatting = Assert.Single(delta.Battings, b => b.PersonId == 5);
            Assert.Equal(1, runnerBatting.TimesCaughtStealing);
        }

        [Fact]
        public void Resolve_RunScored_CreditsRunnerAndResponsiblePitcher_NotCurrentPitcher()
        {
            // Runner (6) was put on base by pitcher 30 (an earlier at-bat, not modeled here --
            // just asserting ResponsiblePitcherId drives the credit); the CURRENT play's
            // pitcher (31) is a reliever who did not put this runner on base.
            var evt = new GameEvent { TeamAtBat = "V", BatterId = 14, PitcherId = 31, EventType = GameEventType.Single };
            var play = Play(
                evt,
                Runner(14, BaseState.BattersBox, BaseState.First),
                Runner(6, BaseState.Third, BaseState.Home, responsiblePitcherId: 30));

            var delta = Resolve(play);

            var scoringRunnerBatting = Assert.Single(delta.Battings, b => b.PersonId == 6);
            Assert.Equal(1, scoringRunnerBatting.Runs);

            var responsiblePitching = Assert.Single(delta.Pitchings, p => p.PersonId == 30);
            Assert.Equal(1, responsiblePitching.Runs);

            var currentPitching = Assert.Single(delta.Pitchings, p => p.PersonId == 31);
            Assert.Equal(0, currentPitching.Runs);
        }

        [Fact]
        public void Resolve_SixFourThreeDoublePlay_BatterGetsGidpAndFieldersGetCredits()
        {
            var evt = new GameEvent { TeamAtBat = "V", BatterId = 15, PitcherId = 25, EventType = GameEventType.GroundOut };
            var play = Play(
                evt,
                Runner(7, BaseState.First, BaseState.Second, isOut: true,
                    credits: [Credit(60, FieldingCreditType.Assist, 1, 6), Credit(64, FieldingCreditType.Putout, 2, 4)]),
                Runner(15, BaseState.BattersBox, BaseState.First, isOut: true,
                    credits: [Credit(64, FieldingCreditType.Assist, 1, 4), Credit(63, FieldingCreditType.Putout, 2, 3)]));

            var delta = Resolve(play);

            var batterBatting = Assert.Single(delta.Battings, b => b.PersonId == 15);
            Assert.Equal(1, batterBatting.GroundedIntoDoublePlay);

            var shortstop = Assert.Single(delta.Fieldings, f => f.PersonId == 60);
            Assert.Equal((byte)6, shortstop.Position);
            Assert.Equal(1, shortstop.Assists);

            // The second baseman (64) is credited twice on the same play -- once as a putout on
            // the forced runner's row, once as an assist on the batter's row -- both rows key
            // to the same (PersonId, Position), so they must accumulate into one FieldingDelta.
            var secondBaseman = Assert.Single(delta.Fieldings, f => f.PersonId == 64);
            Assert.Equal(1, secondBaseman.Putouts);
            Assert.Equal(1, secondBaseman.Assists);

            var firstBaseman = Assert.Single(delta.Fieldings, f => f.PersonId == 63);
            Assert.Equal(1, firstBaseman.Putouts);
        }

        [Fact]
        public void Resolve_SoloHomeRunByStarterWhoFinishesGame_CompleteGameShutout()
        {
            // A minimal "whole game": the same pitcher (40) records every play for the home
            // team across both the first and last plays where the visitor bats -- making them
            // both the starter and the finisher, i.e. a complete game. Zero runs allowed (the
            // batter's own homer is on the visiting team's own pitcher, not this one) keeps it
            // a shutout for pitcher 40.
            var firstPlay = Play(
                new GameEvent { TeamAtBat = "V", BatterId = 16, PitcherId = 40, EventType = GameEventType.GroundOut },
                Runner(16, BaseState.BattersBox, BaseState.First, isOut: true, credits: [Credit(43, FieldingCreditType.Putout, 1, 3)]));
            var lastPlay = Play(
                new GameEvent { TeamAtBat = "V", BatterId = 17, PitcherId = 40, EventType = GameEventType.Strikeout },
                Runner(17, BaseState.BattersBox, BaseState.First, isOut: true));

            var delta = Resolve(firstPlay, lastPlay);

            var pitching = Assert.Single(delta.Pitchings);
            Assert.Equal(1, pitching.GamesPitched);
            Assert.Equal(1, pitching.GamesStarted);
            Assert.Equal(1, pitching.CompleteGames);
            Assert.Equal(0, pitching.GamesFinished);
            Assert.Equal(1, pitching.Shutouts);
            Assert.Equal(0, pitching.Runs);
        }

        [Fact]
        public void Resolve_RelieverFinishesButDidNotStart_GamesFinishedNotCompleteGame()
        {
            var firstPlay = Play(
                new GameEvent { TeamAtBat = "V", BatterId = 16, PitcherId = 40, EventType = GameEventType.Strikeout },
                Runner(16, BaseState.BattersBox, BaseState.First, isOut: true));
            var lastPlay = Play(
                new GameEvent { TeamAtBat = "V", BatterId = 17, PitcherId = 41, EventType = GameEventType.Strikeout },
                Runner(17, BaseState.BattersBox, BaseState.First, isOut: true));

            var delta = Resolve(firstPlay, lastPlay);

            var starter = Assert.Single(delta.Pitchings, p => p.PersonId == 40);
            Assert.Equal(1, starter.GamesStarted);
            Assert.Equal(0, starter.GamesFinished);
            Assert.Equal(0, starter.CompleteGames);

            var reliever = Assert.Single(delta.Pitchings, p => p.PersonId == 41);
            Assert.Equal(0, reliever.GamesStarted);
            Assert.Equal(1, reliever.GamesFinished);
            Assert.Equal(0, reliever.CompleteGames);
        }

        [Fact]
        public void Resolve_InningsPitched_CountsOutsAcrossAllRunnersRegardlessOfRole()
        {
            // One play, two outs recorded (a caught-stealing tacked onto the same pitcher's
            // ledger alongside the batter's own strikeout) -- InningsPitched stores total outs.
            var evt = new GameEvent { TeamAtBat = "V", BatterId = 18, PitcherId = 42, EventType = GameEventType.Strikeout };
            var play = Play(
                evt,
                Runner(18, BaseState.BattersBox, BaseState.First, isOut: true),
                Runner(9, BaseState.First, BaseState.Second, isOut: true));

            var delta = Resolve(play);

            Assert.Equal(2, Assert.Single(delta.Pitchings).InningsPitched);
        }

        [Fact]
        public void Resolve_EarnedRunsSourcedFromExternalDictionary_DefaultsToZeroWhenMissing()
        {
            var evt = new GameEvent { TeamAtBat = "V", BatterId = 19, PitcherId = 50, EventType = GameEventType.Single };
            var play = Play(evt, Runner(19, BaseState.BattersBox, BaseState.First));

            var withData = GameStatisticsResolver.Resolve(
                HomeFranchiseId, VisitorFranchiseId, SeasonYear, [play],
                new Dictionary<int, short> { [50] = 3 });
            Assert.Equal(3, Assert.Single(withData.Pitchings).EarnedRuns);

            var withoutData = GameStatisticsResolver.Resolve(
                HomeFranchiseId, VisitorFranchiseId, SeasonYear, [play],
                new Dictionary<int, short>());
            Assert.Equal(0, Assert.Single(withoutData.Pitchings).EarnedRuns);
        }

        [Fact]
        public void Resolve_ScopeExclusions_SavesAndPassedBallsAlwaysZero()
        {
            var evt = new GameEvent { TeamAtBat = "V", BatterId = 19, PitcherId = 50, EventType = GameEventType.Single };
            var play = Play(evt, Runner(19, BaseState.BattersBox, BaseState.First));

            var delta = Resolve(play);

            Assert.Equal(0, Assert.Single(delta.Pitchings).Saves);
        }
    }
}
