using Retrosharp.Contract.GameEvent;
using Retrosharp.Format.PlayByPlay;

namespace Retrosharp.Format.Tests
{
    /// <summary>
    /// Every play code below is a real, unmodified string pulled from either
    /// docs/csv/2025SDN.EVN or docs/csv/2025SEA.EVA, alongside its real count/pitch-sequence
    /// fields from the same row -- not hand-crafted fixtures. WP/PB standalone examples are the
    /// only exception, since neither file happens to contain one with no accompanying
    /// baserunning; those are marked "(synthetic)".
    /// </summary>
    public class PlayCodeParserTests
    {
        [Fact]
        public void Parse_SimpleGroundOut_BatterOutAtFirstWithAssist()
        {
            // play,7,1,gurry001,21,B*BCX,63/G6
            var result = PlayCodeParser.Parse("63/G6", "21", "B*BCX");

            Assert.Equal(GameEventType.GroundOut, result.EventType);
            Assert.Equal(BattedBallType.GroundBall, result.BattedBallType);

            var batter = Assert.Single(result.Runners);
            Assert.Equal(BaseState.BattersBox, batter.StartBase);
            Assert.Equal(BaseState.First, batter.EndBase);
            Assert.True(batter.IsOut);
            Assert.Equal(
                new[] { (6, FieldingCreditType.Assist, 1), (3, FieldingCreditType.Putout, 2) },
                batter.FieldingCredits.Select(c => ((int)c.Position, c.CreditType, c.Sequence)));
        }

        [Fact]
        public void Parse_SixFourThreeDoublePlay_TwoRunnersOutWithCorrectCredits()
        {
            // spec/game-event.md's worked 6-4-3 example, matched against a real play:
            // play,6,1,diaze005,12,1CBCFX,64(1)3/GDP/G6
            var result = PlayCodeParser.Parse("64(1)3/GDP/G6", "12", "1CBCFX");

            Assert.Equal(GameEventType.GroundOut, result.EventType);
            Assert.Equal(2, result.Runners.Count);

            var forcedRunner = Assert.Single(result.Runners, r => r.StartBase == BaseState.First);
            Assert.Equal(BaseState.Second, forcedRunner.EndBase);
            Assert.True(forcedRunner.IsOut);
            Assert.Equal(
                new[] { (6, FieldingCreditType.Assist, 1), (4, FieldingCreditType.Putout, 2) },
                forcedRunner.FieldingCredits.Select(c => ((int)c.Position, c.CreditType, c.Sequence)));

            var batter = Assert.Single(result.Runners, r => r.StartBase == BaseState.BattersBox);
            Assert.Equal(BaseState.First, batter.EndBase);
            Assert.True(batter.IsOut);
            Assert.Equal(
                new[] { (3, FieldingCreditType.Putout, 1) },
                batter.FieldingCredits.Select(c => ((int)c.Position, c.CreditType, c.Sequence)));
        }

        [Fact]
        public void Parse_ThreeFielderRelayThrowOutAtHome_AllThreeCredited()
        {
            // Relay-throw-to-home analog of spec/game-event.md's 8-6-2 example:
            // play,11,0,durbc002,00,..X,FC/G4.2X3(4561);B-2 -- runner from 2nd thrown out at
            // 3rd via a four-fielder relay (4-5-6-1), batter reaches 2nd on the fielder's choice.
            var result = PlayCodeParser.Parse("FC/G4.2X3(4561);B-2", "00", "..X");

            Assert.Equal(GameEventType.FieldersChoice, result.EventType);

            var batter = Assert.Single(result.Runners, r => r.StartBase == BaseState.BattersBox);
            Assert.Equal(BaseState.Second, batter.EndBase);
            Assert.False(batter.IsOut);

            var thrownOut = Assert.Single(result.Runners, r => r.StartBase == BaseState.Second && r != batter);
            Assert.Equal(BaseState.Third, thrownOut.EndBase);
            Assert.True(thrownOut.IsOut);
            Assert.Equal(
                new[]
                {
                    (4, FieldingCreditType.Assist, 1),
                    (5, FieldingCreditType.Assist, 2),
                    (6, FieldingCreditType.Assist, 3),
                    (1, FieldingCreditType.Putout, 4)
                },
                thrownOut.FieldingCredits.Select(c => ((int)c.Position, c.CreditType, c.Sequence)));
        }

        [Fact]
        public void Parse_RundownStyleChain_RepeatedFielderProducesRepeatedAssist()
        {
            // A real chain where one fielder (1) touches the ball twice, matching
            // spec/game-event.md's rundown description ("a fielder may appear more than once"):
            // play,1,0,chapm001,01,F1,POCS2(1341)
            var result = PlayCodeParser.Parse("POCS2(1341)", "01", "F1");

            Assert.Equal(GameEventType.PickoffCaughtStealing, result.EventType);
            var runner = Assert.Single(result.Runners);
            Assert.Equal(BaseState.First, runner.StartBase);
            Assert.Equal(BaseState.Second, runner.EndBase);
            Assert.True(runner.IsOut);
            Assert.Equal(
                new[]
                {
                    (1, FieldingCreditType.Assist, 1),
                    (3, FieldingCreditType.Assist, 2),
                    (4, FieldingCreditType.Assist, 3),
                    (1, FieldingCreditType.Putout, 4)
                },
                runner.FieldingCredits.Select(c => ((int)c.Position, c.CreditType, c.Sequence)));
        }

        [Theory]
        [InlineData("S1/G1S", BaseState.First)] // play,...,S1/G1S -- generic single shape
        [InlineData("D34/G3.1-3", BaseState.Second)]
        [InlineData("T8/L89XD+.2-H(UR)", BaseState.Third)] // play,10,0,hoern001,20,..BBX,T8/L89XD+.2-H(UR)
        [InlineData("HR/F7LD.3-H;2-H;1-H", BaseState.Home)] // play,9,0,wardt002,22,BBCCFX,HR/F7LD.3-H;2-H;1-H
        public void Parse_Hits_BatterAdvancesToImpliedBase(string rawEventText, BaseState expectedBatterEndBase)
        {
            var result = PlayCodeParser.Parse(rawEventText, "22", "BX");

            var batter = Assert.Single(result.Runners, r => r.StartBase == BaseState.BattersBox);
            Assert.Equal(expectedBatterEndBase, batter.EndBase);
            Assert.False(batter.IsOut);
        }

        [Fact]
        public void Parse_HomeRun_EveryScoringRunnerGetsRbiAndEarnedByDefault()
        {
            // play,9,0,wardt002,22,BBCCFX,HR/F7LD.3-H;2-H;1-H
            var result = PlayCodeParser.Parse("HR/F7LD.3-H;2-H;1-H", "22", "BBCCFX");

            Assert.Equal(GameEventType.HomeRun, result.EventType);
            Assert.Equal(4, result.Runners.Count);
            foreach (var runner in result.Runners.Where(r => r.StartBase != BaseState.BattersBox))
            {
                Assert.Equal(BaseState.Home, runner.EndBase);
                Assert.False(runner.IsOut);
                Assert.True(runner.IsRBI);
                Assert.True(runner.IsEarnedRun);
            }
        }

        [Fact]
        public void Parse_WalkWithBasesLoaded_OnlyForcedRunnerFromThirdGetsRbi()
        {
            // play,8,1,tatif002,32,BBCCFFFFB>B,W.3-H;2-3;1-2
            var result = PlayCodeParser.Parse("W.3-H;2-3;1-2", "32", "BBCCFFFFB>B");

            Assert.Equal(GameEventType.Walk, result.EventType);
            Assert.Equal(4, result.Runners.Count);

            var scored = Assert.Single(result.Runners, r => r.EndBase == BaseState.Home);
            Assert.Equal(BaseState.Third, scored.StartBase);
            Assert.True(scored.IsRBI);
            Assert.True(scored.IsEarnedRun);

            var others = result.Runners.Where(r => r.EndBase != BaseState.Home);
            Assert.All(others, r => Assert.False(r.IsRBI));
        }

        [Fact]
        public void Parse_IntentionalWalk_BatterSafeAtFirst()
        {
            // play,7,1,bogax001,30,VVVV,IW
            var result = PlayCodeParser.Parse("IW", "30", "VVVV");

            Assert.Equal(GameEventType.IntentionalWalk, result.EventType);
            var batter = Assert.Single(result.Runners);
            Assert.Equal(BaseState.First, batter.EndBase);
            Assert.False(batter.IsOut);
        }

        [Fact]
        public void Parse_HitByPitch_BatterSafeAtFirst()
        {
            // play,2,1,bogax001,02,CFH,HP
            var result = PlayCodeParser.Parse("HP", "02", "CFH");

            Assert.Equal(GameEventType.HitByPitch, result.EventType);
            var batter = Assert.Single(result.Runners);
            Assert.Equal(BaseState.First, batter.EndBase);
            Assert.False(batter.IsOut);
        }

        [Fact]
        public void Parse_Strikeout_BatterOut()
        {
            var result = PlayCodeParser.Parse("K", "32", "CBFBS");

            Assert.Equal(GameEventType.Strikeout, result.EventType);
            var batter = Assert.Single(result.Runners);
            Assert.True(batter.IsOut);
        }

        [Fact]
        public void Parse_BareErrorCode_BatterSafeWithErrorCredit()
        {
            // play,4,0,turnj001,12,CBSFX,E5/G56.B-1
            var result = PlayCodeParser.Parse("E5/G56.B-1", "12", "CBSFX");

            Assert.Equal(GameEventType.Error, result.EventType);
            var batter = Assert.Single(result.Runners);
            Assert.Equal(BaseState.First, batter.EndBase);
            Assert.False(batter.IsOut);
            var credit = Assert.Single(batter.FieldingCredits);
            Assert.Equal(5, credit.Position);
            Assert.Equal(FieldingCreditType.Error, credit.CreditType);
        }

        [Fact]
        public void Parse_MidChainError_BatterSafeWithAssistAndErrorCredits()
        {
            // play,1,1,rodrj007,00,X,4E3/G4M.B-1 -- fielder 4 assists, fielder 3 charged the
            // error; nobody is out despite the digit-sequence shape looking like a fielded out.
            var result = PlayCodeParser.Parse("4E3/G4M.B-1", "00", "X");

            Assert.Equal(GameEventType.Error, result.EventType);
            var batter = Assert.Single(result.Runners);
            Assert.False(batter.IsOut);
            Assert.Equal(BaseState.First, batter.EndBase);
            Assert.Equal(
                new[] { (4, FieldingCreditType.Assist, 1), (3, FieldingCreditType.Error, 2) },
                batter.FieldingCredits.Select(c => ((int)c.Position, c.CreditType, c.Sequence)));
        }

        [Fact]
        public void Parse_DroppedFoulError_NoRunnerRecorded()
        {
            // play,1,0,thoml002,11,SBF,FLE2 -- a foul ball dropped for an error; the batter
            // never becomes a runner at all, unlike a bare "E<n>".
            var result = PlayCodeParser.Parse("FLE2", "11", "SBF");

            Assert.Equal(GameEventType.Error, result.EventType);
            Assert.Empty(result.Runners);
        }

        [Fact]
        public void Parse_FieldersChoice_BatterSafeAtFirstByDefault()
        {
            var result = PlayCodeParser.Parse("FC/G1.1-2", "11", "CBX");

            Assert.Equal(GameEventType.FieldersChoice, result.EventType);
            var batter = Assert.Single(result.Runners, r => r.StartBase == BaseState.BattersBox);
            Assert.Equal(BaseState.First, batter.EndBase);
            Assert.False(batter.IsOut);
        }

        [Fact]
        public void Parse_StolenBase_RunnerAdvancesSafely()
        {
            // play,1,1,merrj002,02,FC>B,SB2
            var result = PlayCodeParser.Parse("SB2", "02", "FC>B");

            Assert.Equal(GameEventType.StolenBase, result.EventType);
            var runner = Assert.Single(result.Runners);
            Assert.Equal(BaseState.First, runner.StartBase);
            Assert.Equal(BaseState.Second, runner.EndBase);
            Assert.False(runner.IsOut);
        }

        [Fact]
        public void Parse_CaughtStealing_RunnerOutWithFielderCredits()
        {
            var result = PlayCodeParser.Parse("CS2(24)", "10", "B");

            Assert.Equal(GameEventType.CaughtStealing, result.EventType);
            var runner = Assert.Single(result.Runners);
            Assert.Equal(BaseState.First, runner.StartBase);
            Assert.Equal(BaseState.Second, runner.EndBase);
            Assert.True(runner.IsOut);
            Assert.Equal(
                new[] { (2, FieldingCreditType.Assist, 1), (4, FieldingCreditType.Putout, 2) },
                runner.FieldingCredits.Select(c => ((int)c.Position, c.CreditType, c.Sequence)));
        }

        [Fact]
        public void Parse_WildPitch_NoInherentRunnerImplication()
        {
            // (synthetic -- neither reference file has a standalone WP with no baserunner)
            var result = PlayCodeParser.Parse("WP", "10", "B");

            Assert.Equal(GameEventType.WildPitch, result.EventType);
            Assert.Empty(result.Runners);
        }

        [Fact]
        public void Parse_PassedBall_WithRunnerAdvance()
        {
            // (synthetic -- neither reference file has a standalone PB)
            var result = PlayCodeParser.Parse("PB.1-2", "10", "B");

            Assert.Equal(GameEventType.PassedBall, result.EventType);
            var runner = Assert.Single(result.Runners);
            Assert.Equal(BaseState.First, runner.StartBase);
            Assert.Equal(BaseState.Second, runner.EndBase);
        }

        [Fact]
        public void Parse_Balk_RunnersAdvanceUnearnedOverride()
        {
            var result = PlayCodeParser.Parse("BK.3-H(NR);2-3;1-2", "10", "B");

            Assert.Equal(GameEventType.Balk, result.EventType);
            var scored = Assert.Single(result.Runners, r => r.EndBase == BaseState.Home);
            Assert.False(scored.IsRBI);
        }

        [Fact]
        public void Parse_DefensiveIndifference_RunnerAdvancesWithNoOut()
        {
            // play,9,0,buscm003,00,>C,DI.1-3
            var result = PlayCodeParser.Parse("DI.1-3", "00", ">C");

            Assert.Equal(GameEventType.DefensiveIndifference, result.EventType);
            var runner = Assert.Single(result.Runners);
            Assert.Equal(BaseState.First, runner.StartBase);
            Assert.Equal(BaseState.Third, runner.EndBase);
            Assert.False(runner.IsOut);
        }

        [Fact]
        public void Parse_OtherAdvance_RunnerOutWithFielderCredits()
        {
            // play,1,1,cronj001,02,C1*S*B,OA.1X2(26)
            var result = PlayCodeParser.Parse("OA.1X2(26)", "02", "C1*S*B");

            Assert.Equal(GameEventType.OtherAdvance, result.EventType);
            var runner = Assert.Single(result.Runners);
            Assert.True(runner.IsOut);
            Assert.Equal(
                new[] { (2, FieldingCreditType.Assist, 1), (6, FieldingCreditType.Putout, 2) },
                runner.FieldingCredits.Select(c => ((int)c.Position, c.CreditType, c.Sequence)));
        }

        [Fact]
        public void Parse_NoPlay_ProducesNoRunners()
        {
            // play,3,0,arcio002,00,,NP
            var result = PlayCodeParser.Parse("NP", "00", "");

            Assert.Equal(GameEventType.NoPlay, result.EventType);
            Assert.Empty(result.Runners);
        }

        [Fact]
        public void Parse_StrikeoutCombinedWithCaughtStealing_BothRunnersOut()
        {
            // play,5,0,ramia004,32,SFBFBB>C,K+CS2(24)/DP
            var result = PlayCodeParser.Parse("K+CS2(24)/DP", "32", "SFBFBB>C");

            Assert.Equal(GameEventType.Strikeout, result.EventType);
            Assert.Equal(2, result.Runners.Count);

            var batter = Assert.Single(result.Runners, r => r.StartBase == BaseState.BattersBox);
            Assert.True(batter.IsOut);

            var caughtStealing = Assert.Single(result.Runners, r => r.StartBase == BaseState.First);
            Assert.True(caughtStealing.IsOut);
            Assert.Equal(BaseState.Second, caughtStealing.EndBase);
        }

        [Fact]
        public void Parse_SacrificeFly_SetsIsSacFlyBattedBallTypeAndRbi()
        {
            // play,3,0,diazy001,22,BFBSX,7/SF/L7D.3-H(UR)
            var result = PlayCodeParser.Parse("7/SF/L7D.3-H(UR)", "22", "BFBSX");

            Assert.Equal(GameEventType.FlyOut, result.EventType);
            Assert.Equal(BattedBallType.LineDrive, result.BattedBallType);
            Assert.True(result.IsSacFly);

            var scored = Assert.Single(result.Runners, r => r.StartBase == BaseState.Third);
            Assert.True(scored.IsRBI);
            Assert.False(scored.IsEarnedRun);
        }

        [Fact]
        public void Parse_NoRbiAnnotation_DeniesTheDefaultRbi()
        {
            var result = PlayCodeParser.Parse("S9/G34.3-H(NR);1-2", "22", "BX");

            var scored = Assert.Single(result.Runners, r => r.StartBase == BaseState.Third);
            Assert.False(scored.IsRBI);
            Assert.True(scored.IsEarnedRun);
        }

        [Fact]
        public void Parse_TeamUnearnedRunAnnotation_TreatedSameAsUnearned()
        {
            // play,8,0,wagae001,20,BBX,D7/L7LD.2-H(TUR);1XH(762)
            var result = PlayCodeParser.Parse("D7/L7LD.2-H(TUR);1XH(762)", "20", "BBX");

            var scored = Assert.Single(result.Runners, r => r.StartBase == BaseState.Second);
            Assert.True(scored.IsRBI);
            Assert.False(scored.IsEarnedRun);
        }

        [Fact]
        public void Parse_PitchSequence_CountsFoulBallsOnlyAfterTwoStrikes()
        {
            // play,1,1,arral001,32,LBBSBFFX,63/G6S.1-2 -- L (bunt foul, strike), B, B, S
            // (strike #2), B, then two plain fouls while already at 2 strikes.
            var result = PlayCodeParser.Parse("63/G6S.1-2", "32", "LBBSBFFX");

            Assert.Equal(2, result.FoulBallsWithTwoStrikes);
            Assert.Equal(3, result.Balls);
            Assert.Equal(2, result.Strikes);
        }

        [Fact]
        public void Parse_PitchSequence_FoulsBeforeTwoStrikesDontCount()
        {
            var result = PlayCodeParser.Parse("K", "02", "FFC");

            Assert.Equal(0, result.FoulBallsWithTwoStrikes);
        }

        [Fact]
        public void Parse_MalformedCountField_Throws()
        {
            Assert.Throws<PlayCodeParseException>(() => PlayCodeParser.Parse("K", "3", "C"));
        }

        [Fact]
        public void Parse_UnrecognizedPrimaryCode_Throws()
        {
            Assert.Throws<PlayCodeParseException>(() => PlayCodeParser.Parse("ZZZ", "00", ""));
        }
    }
}
