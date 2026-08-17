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

            // spec/game-event.md's Data Model explicitly calls out that the second baseman is
            // credited twice on this play: a putout on the forced runner (above), then an
            // assist on the batter here, before the first baseman's putout.
            var batter = Assert.Single(result.Runners, r => r.StartBase == BaseState.BattersBox);
            Assert.Equal(BaseState.First, batter.EndBase);
            Assert.True(batter.IsOut);
            Assert.Equal(
                new[] { (4, FieldingCreditType.Assist, 1), (3, FieldingCreditType.Putout, 2) },
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

        [Fact]
        public void Parse_PickoffWithThrowingError_RunnerSafeAndAdvancesExtraBase()
        {
            // play,6,1,sheeg001,20,B*B2,PO2(E1/TH).2-3 -- a real pickoff attempt at second with
            // a throwing error by the pitcher (fielder 1); the runner is safe (not out) and
            // advances an extra base to third on the misplay. Regression test: PO<base>'s
            // parenthetical was being parsed as if it were always a fielder-putout chain
            // (like a fielded out's "(<fielders>)"), so "E1/TH" -- an error annotation, a
            // different grammar -- was read character-by-character as fielder digits,
            // producing garbage Position values from non-digit characters (confirmed against
            // this real play: 'E'-'0'=21, '/'-'0' wraps to 255, 'T'-'0'=36, 'H'-'0'=24).
            var result = PlayCodeParser.Parse("PO2(E1/TH).2-3", "20", "B*B2");

            Assert.Equal(GameEventType.Pickoff, result.EventType);
            var runner = Assert.Single(result.Runners);
            Assert.Equal(BaseState.Second, runner.StartBase);
            Assert.Equal(BaseState.Third, runner.EndBase);
            Assert.False(runner.IsOut);
            Assert.Equal(
                new[] { (1, FieldingCreditType.Error, 1) },
                runner.FieldingCredits.Select(c => ((int)c.Position, c.CreditType, c.Sequence)));
        }

        [Fact]
        public void Parse_RunnerOutAdvanceWithEmbeddedError_SafeOnErrorNotPutOut()
        {
            // play,5,1,sheeg001,22,BCFBX,FC4/G34.2-3;1X2(4E6);B-1 -- a real fielder's choice.
            // "1X2(4E6)" reads as an out (fielder 4 assists, fielder 6 completes it at second),
            // but the trailing credit is an Error, not a Putout -- the throw that would have
            // completed the force was itself muffed, so the runner is actually safe at second.
            // Regression test, two bugs found chasing the same play: (1) the runner-out branch
            // called the same raw fielder-chain parser used for plain putout chains, which
            // didn't recognize "4E6"'s embedded error and read 'E' itself as a digit,
            // producing a garbage Position (69 - 48 = 21); (2) once the embedded error parsed
            // correctly, the runner was still marked out despite the trailing credit being an
            // Error rather than a Putout -- confirmed wrong by an outs-count check against the
            // full real half-inning: treating this as the inning's 2nd out would make the
            // following strikeout the 3rd, yet the real file still has another batter (and
            // another play) after that in the same half-inning -- an impossible 4th out.
            var result = PlayCodeParser.Parse("FC4/G34.2-3;1X2(4E6);B-1", "22", "BCFBX");

            var runner = Assert.Single(result.Runners, r => r.StartBase == BaseState.First);
            Assert.Equal(BaseState.Second, runner.EndBase);
            Assert.False(runner.IsOut);
            Assert.Equal(
                new[] { (4, FieldingCreditType.Assist, 1), (6, FieldingCreditType.Error, 2) },
                runner.FieldingCredits.Select(c => ((int)c.Position, c.CreditType, c.Sequence)));
        }

        [Fact]
        public void Parse_DoubleSteal_BothRunnersAdvance()
        {
            // play,7,1,merrj002,00,>B,SB3;SB2 -- a real double steal. Regression test: the
            // semicolon-joined-primary-codes branch previously used "result ??= ParseSingleCode(...)",
            // whose "??=" short-circuits and skips *calling* ParseSingleCode entirely once
            // "result" is non-null -- silently dropping every steal after the first.
            var result = PlayCodeParser.Parse("SB3;SB2", "00", ">B");

            Assert.Equal(GameEventType.StolenBase, result.EventType);
            Assert.Equal(2, result.Runners.Count);
            Assert.Single(result.Runners, r => r.StartBase == BaseState.Second && r.EndBase == BaseState.Third);
            Assert.Single(result.Runners, r => r.StartBase == BaseState.First && r.EndBase == BaseState.Second);
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
        public void Parse_BareStrikeout_CreditsCatcherWithPutout()
        {
            // Regression test for spec/defects.md's "Missing catcher putout on strikeouts": a
            // bare "K" carries no fielder digits, but standard scoring still credits the catcher
            // (position 2) with the putout -- confirmed missing against real data in
            // docs/csv/2025HOU.EVA, where every bare "K" undercounted GameFieldingStatistics.
            // Putouts by exactly one relative to the Game Log Parser's independently-sourced
            // totals.
            var result = PlayCodeParser.Parse("K", "32", "CBFBS");

            var batter = Assert.Single(result.Runners);
            Assert.Equal(
                new[] { (2, FieldingCreditType.Putout, 1) },
                batter.FieldingCredits.Select(c => ((int)c.Position, c.CreditType, c.Sequence)));
        }

        [Fact]
        public void Parse_DroppedThirdStrikeThrownOut_DoesNotDoubleCreditCatcherPutout()
        {
            // play,4,0,nimmb001,22,BSSBF*S,K.BX1(23) -- docs/csv/2025HOU.EVA: a dropped third
            // strike where the batter is thrown out at first by the catcher(2)-to-first-
            // baseman(3) relay. The explicit fielder chain from the advance segment is the real
            // disposition; the implicit catcher-putout-on-strikeout rule must not also fire and
            // give the catcher a second, phantom putout.
            var result = PlayCodeParser.Parse("K.BX1(23)", "22", "BSSBF*S");

            var batter = Assert.Single(result.Runners);
            Assert.True(batter.IsOut);
            Assert.Equal(
                new[] { (2, FieldingCreditType.Assist, 1), (3, FieldingCreditType.Putout, 2) },
                batter.FieldingCredits.Select(c => ((int)c.Position, c.CreditType, c.Sequence)));
        }

        [Fact]
        public void Parse_StrikeoutWithWildPitchBatterReachesSafely_NoCatcherPutout()
        {
            // play,3,1,parei001,12,CB>F.S,K+WP.1-2;B-1 -- docs/csv/2025HOU.EVA: strikeout on a
            // wild pitch where the batter reaches first safely. IsOut ends up false via the
            // explicit "B-1" advance, so there's genuinely no putout on this play at all.
            var result = PlayCodeParser.Parse("K+WP.1-2;B-1", "12", "CB>F.S");

            var batter = Assert.Single(result.Runners, r => r.StartBase == BaseState.BattersBox);
            Assert.False(batter.IsOut);
            Assert.Empty(batter.FieldingCredits);
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
            // never becomes a runner at all, unlike a bare "E<n>". Distinct
            // GameEventType.FoulBallError (not Error) so GameStatisticsResolver doesn't count
            // this as its own plate appearance/at-bat -- see spec/defects.md,
            // "PlateAppearances/AtBats overcounted on a foul ball dropped for an error."
            var result = PlayCodeParser.Parse("FLE2", "11", "SBF");

            Assert.Equal(GameEventType.FoulBallError, result.EventType);
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
        public void Parse_CaughtStealingWithErrorOnRelay_RunnerSafeNotOut()
        {
            // play,5,0,hummc001,32,CFVB*B.>C,K+CS3(2E5).1-2 -- docs/csv/2025ATH.EVA. The fielder
            // chain "2E5" ends in an error (fielder 5's throw), meaning the runner reached third
            // safely rather than being retired -- the same rule ApplyAdvanceSegment already
            // applies to explicit "X" advances (see its "1X2(4E6)" test). Before this was fixed,
            // ParseCaughtStealingLike unconditionally marked the runner out and dropped him from
            // the tracker, which two plays later threw PlayCodeParseException-adjacent
            // InvalidOperationException: a following play required a runner on Third the
            // resolver no longer had any record of.
            var result = PlayCodeParser.Parse("K+CS3(2E5).1-2", "32", "CFVB*B.>C");

            Assert.Equal(GameEventType.Strikeout, result.EventType);
            Assert.Equal(GameEventType.CaughtStealing, result.SecondaryEventType);

            var caughtStealingRunner = Assert.Single(result.Runners, r => r.StartBase == BaseState.Second);
            Assert.Equal(BaseState.Third, caughtStealingRunner.EndBase);
            Assert.False(caughtStealingRunner.IsOut);
            Assert.Equal(
                new[] { (2, FieldingCreditType.Assist, 1), (5, FieldingCreditType.Error, 2) },
                caughtStealingRunner.FieldingCredits.Select(c => ((int)c.Position, c.CreditType, c.Sequence)));

            var advancingRunner = Assert.Single(result.Runners, r => r.StartBase == BaseState.First);
            Assert.Equal(BaseState.Second, advancingRunner.EndBase);
            Assert.False(advancingRunner.IsOut);
        }

        [Fact]
        public void Parse_CaughtStealingWithStructuredErrorAnnotation_RunnerSafeNotOut()
        {
            // play,1,0,wallt003,21,1CBB1N,CS2(E1/TH).1-3 -- docs/csv/2025BAL.EVA. "(E1/TH)" is a
            // structured error annotation (the pitcher's throw on the attempt was itself the
            // error), not a raw fielder-digit chain -- the same grammar PO already handled for
            // its own "(E1/TH)" case. Before this was fixed, ParseCaughtStealingLike always
            // treated the parenthetical as a raw chain and crashed on the non-digit '/', 'T',
            // 'H' characters.
            var result = PlayCodeParser.Parse("CS2(E1/TH).1-3", "21", "1CBB1N");

            Assert.Equal(GameEventType.CaughtStealing, result.EventType);

            var runner = Assert.Single(result.Runners, r => r.StartBase == BaseState.First);
            Assert.Equal(BaseState.Third, runner.EndBase);
            Assert.False(runner.IsOut);
            Assert.Equal(
                new[] { (1, FieldingCreditType.Error, 1) },
                runner.FieldingCredits.Select(c => ((int)c.Position, c.CreditType, c.Sequence)));
        }

        [Fact]
        public void Parse_CatcherInterferenceWithErrorModifier_CreditsCatcherError()
        {
            // play,1,0,sprig001,01,CX,C/E2.2-3;1-2;B-1 -- docs/csv/2025BOS.EVA. "E2" here is a
            // bare modifier (after "/", not a primary code and not an advance segment's "(E$)"
            // annotation) -- Catcher's Interference where the catcher also committed a
            // subsequent throwing error. ApplyModifiers had no case for "E<digit>" at all, so
            // this credit was silently dropped, undercounting GameFieldingStatistics.Errors by
            // exactly 1 in every affected game.
            var result = PlayCodeParser.Parse("C/E2.2-3;1-2;B-1", "01", "CX");

            Assert.Equal(GameEventType.CatcherInterference, result.EventType);

            var batter = Assert.Single(result.Runners, r => r.StartBase == BaseState.BattersBox);
            Assert.Equal(
                new[] { (2, FieldingCreditType.Error, 1) },
                batter.FieldingCredits.Select(c => ((int)c.Position, c.CreditType, c.Sequence)));
        }

        [Fact]
        public void Parse_StolenBaseWithBystanderRunnerScoringOnError_OnlyStealerFlagged()
        {
            // play,3,1,abrew002,02,CS.>B,SB2.3-H(E2/TH)(NR)(UR);1-3 -- docs/csv/2025BOS.EVA. The
            // steal's throw gets away, letting a *different* runner (who started at Third) score
            // as a side effect. Regression test for spec/defects.md's "Discrepancy Issues in
            // 2025BOS.EVA": only the runner whose own disposition actually came from the "SB"
            // code should carry IsStolenBase -- GameStatisticsResolver previously credited
            // Batting.StolenBases to *every* non-batter runner in a StolenBase-typed play,
            // overcounting whenever a bystander runner merely advanced alongside the steal.
            var result = PlayCodeParser.Parse("SB2.3-H(E2/TH)(NR)(UR);1-3", "02", "CS.>B");

            Assert.Equal(GameEventType.StolenBase, result.EventType);

            var stealer = Assert.Single(result.Runners, r => r.StartBase == BaseState.First);
            Assert.Equal(BaseState.Third, stealer.EndBase);
            Assert.True(stealer.IsStolenBase);

            var scorer = Assert.Single(result.Runners, r => r.StartBase == BaseState.Third);
            Assert.Equal(BaseState.Home, scorer.EndBase);
            Assert.False(scorer.IsStolenBase);
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
            Assert.Equal(GameEventType.CaughtStealing, result.SecondaryEventType);
            Assert.Equal(2, result.Runners.Count);

            var batter = Assert.Single(result.Runners, r => r.StartBase == BaseState.BattersBox);
            Assert.True(batter.IsOut);

            var caughtStealing = Assert.Single(result.Runners, r => r.StartBase == BaseState.First);
            Assert.True(caughtStealing.IsOut);
            Assert.Equal(BaseState.Second, caughtStealing.EndBase);
        }

        [Fact]
        public void Parse_StrikeoutCombinedWithStolenBase_SecondaryEventTypeSet()
        {
            var result = PlayCodeParser.Parse("K+SB2", "22", "CBBFS");

            Assert.Equal(GameEventType.Strikeout, result.EventType);
            Assert.Equal(GameEventType.StolenBase, result.SecondaryEventType);

            var stealer = Assert.Single(result.Runners, r => r.StartBase == BaseState.First);
            Assert.False(stealer.IsOut);
            Assert.Equal(BaseState.Second, stealer.EndBase);
        }

        [Fact]
        public void Parse_StrikeoutCombinedWithWildPitch_SecondaryEventTypeSetAndNoExtraRunner()
        {
            var result = PlayCodeParser.Parse("K+WP", "22", "CBBFS");

            Assert.Equal(GameEventType.Strikeout, result.EventType);
            Assert.Equal(GameEventType.WildPitch, result.SecondaryEventType);
        }

        [Fact]
        public void Parse_SingleWithNoCombinator_SecondaryEventTypeIsNull()
        {
            var result = PlayCodeParser.Parse("S9/G34", "21", "B*BCX");

            Assert.Null(result.SecondaryEventType);
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
        public void Parse_LineDriveBunt_ResolvesToFlyOutWithLineDriveBattedBallType()
        {
            // play,9,1,barrj004,01,LX,1/BL1S -- docs/csv/2025ARI.EVN. "BL" (line drive bunt) is
            // the third documented bunt-trajectory modifier alongside BG/BP; a fielded out whose
            // only modifier is "BL..." was throwing PlayCodeParseException for lacking a
            // trajectory modifier before this was recognized.
            var result = PlayCodeParser.Parse("1/BL1S", "01", "LX");

            Assert.Equal(GameEventType.FlyOut, result.EventType);
            Assert.Equal(BattedBallType.LineDrive, result.BattedBallType);
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
        public void Parse_WildPitchAdvanceAnnotation_DoesNotThrowAndLeavesRunnerUnaffected()
        {
            // play,9,0,lee-b002,22,BFB2C>B,SB3.1-2(WP) -- docs/csv/2025TEX.EVA:12992. "(WP)" is a
            // documented Retrosheet advance annotation ("an alternative way of indicating wild
            // pitches and passed balls"), purely informational -- it must not throw, and must not
            // affect IsRBI/IsEarnedRun/fielding credits any differently than a plain "1-2" would.
            var result = PlayCodeParser.Parse("SB3.1-2(WP)", "22", "BFB2C>B");

            var advanced = Assert.Single(result.Runners, r => r.StartBase == BaseState.First);
            Assert.Equal(BaseState.Second, advanced.EndBase);
            Assert.False(advanced.IsOut);
            Assert.Empty(advanced.FieldingCredits);
        }

        [Fact]
        public void Parse_PassedBallAdvanceAnnotation_DoesNotThrow()
        {
            // (synthetic -- no real "(PB)" advance annotation observed in the reference files,
            // but it's documented alongside "(WP)" as the same kind of informational tag and
            // shares the same code path).
            var result = PlayCodeParser.Parse("S7/G6.2-3(PB)", "10", "BX");

            var advanced = Assert.Single(result.Runners, r => r.StartBase == BaseState.Second);
            Assert.Equal(BaseState.Third, advanced.EndBase);
            Assert.False(advanced.IsOut);
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

        [Fact]
        public void Parse_BareBatterInterference_ResolvesToGroundOutWithNoTrajectoryModifier()
        {
            // play,1,0,campk002,11,*B1CS,2/BINT -- docs/csv/2025TBA.EVA:2165. Batter interference
            // is an out on the batter with no batted ball at all, so unlike every other digit-led
            // fielded-out code there's no trajectory modifier to find -- confirmed crashing
            // before this fix ("Fielded-out code has no trajectory modifier...").
            var result = PlayCodeParser.Parse("2/BINT", "11", "*B1CS");

            Assert.Equal(GameEventType.GroundOut, result.EventType);
            var batter = Assert.Single(result.Runners);
            Assert.True(batter.IsOut);
            Assert.Equal(
                new[] { (2, FieldingCreditType.Putout, 1) },
                batter.FieldingCredits.Select(c => ((int)c.Position, c.CreditType, c.Sequence)));
        }

        [Fact]
        public void Parse_BatterInterferenceWithRealTrajectory_TrajectoryTakesPriority()
        {
            // play,6,1,olsom001,31,BCBBX,2/P2F/FL/BINT -- docs/csv/2025ATL.EVN:12983. When a real
            // trajectory modifier IS present alongside "BINT" (this one happened on a caught pop
            // fly), it must still win -- the BINT fallback only applies when nothing else
            // determined a trajectory.
            var result = PlayCodeParser.Parse("2/P2F/FL/BINT", "31", "BCBBX");

            Assert.Equal(GameEventType.FlyOut, result.EventType);
            Assert.Equal(BattedBallType.PopUp, result.BattedBallType);
        }

        [Fact]
        public void Parse_RunnerInterferenceAdvanceAnnotation_DoesNotThrow()
        {
            // (synthetic -- no real "(fielder/INT)" advance annotation observed in the reference
            // files, but it's Retrosheet's own documented alternate notation for runner
            // interference: "S/L9S.3-H;2X3(5/INT);1-2" -- "An alternative way of writing this is
            // (5/INT)." Confirmed crashing ParseFielderChain on the '/' before this fix.
            var result = PlayCodeParser.Parse("S/L9S.3-H;2X3(5/INT);1-2", "00", "X");

            var thrownOut = Assert.Single(result.Runners, r => r.StartBase == BaseState.Second);
            Assert.Equal(BaseState.Third, thrownOut.EndBase);
            Assert.True(thrownOut.IsOut);
            Assert.Equal(
                new[] { (5, FieldingCreditType.Putout, 1) },
                thrownOut.FieldingCredits.Select(c => ((int)c.Position, c.CreditType, c.Sequence)));
        }

        [Fact]
        public void Parse_DroppedThirdStrikeThrownToFirst_CreditsCatcherAssistAndFirstBasePutout()
        {
            // play,6,1,wynnm001,22,..BBFCFS,K23 -- Retrosheet's own worked example: "A dropped
            // third strike with a putout at first base is given by the event K23." Before this
            // fix, code.StartsWith("K") matched "K23" the same as a bare "K", silently dropping
            // the "23" suffix and letting the catcher be wrongly credited an unassisted putout.
            var result = PlayCodeParser.Parse("K23", "22", "..BBFCFS");

            Assert.Equal(GameEventType.Strikeout, result.EventType);
            var batter = Assert.Single(result.Runners);
            Assert.True(batter.IsOut);
            Assert.Equal(
                new[] { (2, FieldingCreditType.Assist, 1), (3, FieldingCreditType.Putout, 2) },
                batter.FieldingCredits.Select(c => ((int)c.Position, c.CreditType, c.Sequence)));
        }

        [Fact]
        public void Parse_BareStrikeout_StillCreditsCatcherUnassistedPutout()
        {
            // Regression guard for the K23 fix above: a plain "K" (no fielder-chain suffix) must
            // still fall through to Parse()'s existing fallback and get the catcher's unassisted
            // putout, unchanged.
            var result = PlayCodeParser.Parse("K", "22", "CFBS");

            var batter = Assert.Single(result.Runners);
            Assert.Equal(
                new[] { (2, FieldingCreditType.Putout, 1) },
                batter.FieldingCredits.Select(c => ((int)c.Position, c.CreditType, c.Sequence)));
        }

        [Fact]
        public void Parse_UnknownPlayPlaceholder99_ProducesNoFieldingCredits()
        {
            // (synthetic -- no real "99" placeholder observed in the reference files, which are
            // all modern (2025) data; this is Retrosheet's documented marker for very old/
            // incomplete games with unrecorded fielders: "the double digit combination 99, which
            // cannot arise in play, is used to code unknown plays... No assist or putout credits
            // are given." Before this fix, "99" was treated as two real fielder-9 (right field)
            // credits -- an assist and a putout.
            var result = PlayCodeParser.Parse("99/G", "00", "X");

            Assert.Equal(GameEventType.GroundOut, result.EventType);
            var batter = Assert.Single(result.Runners);
            Assert.True(batter.IsOut);
            Assert.Empty(batter.FieldingCredits);
        }
    }
}
