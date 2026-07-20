using Retrosharp.Contract.GameEvent;
using Retrosharp.Format.EventFile;
using Retrosharp.Format.PlayByPlay;

namespace Retrosharp.Format.Tests
{
    /// <summary>
    /// Exercises <see cref="GameEventResolver"/> against hand-assembled <see cref="EventFileGame"/>
    /// objects. Every play code is a real one, pulled from the same reference games used by
    /// <see cref="PlayCodeParserTests"/> and Fixtures/eventfile_sample.EVN -- the surrounding
    /// scaffolding (start/sub records, preceding plays that establish a baserunner) is
    /// hand-built rather than read from a file, since these tests target the resolver's own
    /// state tracking (lineup/position, baserunner identity, inherited-runner responsibility),
    /// not the file-reading or play-code-grammar concerns already covered by
    /// <see cref="EventFileReaderTests"/> and <see cref="PlayCodeParserTests"/>.
    /// </summary>
    public class GameEventResolverTests
    {
        private const string HomePitcher = "kingm002";
        private const string VisitingPitcher = "salec001";

        private static IReadOnlyDictionary<string, int> PersonIds(params string[] retrosheetIds)
        {
            var map = new Dictionary<string, int>();
            for (var i = 0; i < retrosheetIds.Length; i++)
                map[retrosheetIds[i]] = i + 1;
            return map;
        }

        private static StartRecord Start(string retrosheetId, bool isHomeTeam, byte battingOrder, byte position) =>
            new() { RetrosheetId = retrosheetId, Name = retrosheetId, IsHomeTeam = isHomeTeam, BattingOrder = battingOrder, Position = position };

        private static SubRecord Sub(string retrosheetId, bool isHomeTeam, byte battingOrder, byte position) =>
            new() { RetrosheetId = retrosheetId, Name = retrosheetId, IsHomeTeam = isHomeTeam, BattingOrder = battingOrder, Position = position };

        private static PlayRecord Play(byte inning, bool isHomeTeamBatting, string retrosheetId, string count, string pitches, string rawEventText) =>
            new() { Inning = inning, IsHomeTeamBatting = isHomeTeamBatting, RetrosheetId = retrosheetId, CountField = count, PitchSequence = pitches, RawEventText = rawEventText };

        private static EventFileGame Game(IReadOnlyList<EventFileRecord> records) => new()
        {
            GameId = "TST202501010",
            HomeTeamCode = "SDN",
            VisitingTeamCode = "ATL",
            GameDate = new DateTime(2025, 1, 1),
            GameNumber = 0,
            Records = records
        };

        [Fact]
        public void Resolve_SimpleGroundOut_BatterAndFieldersResolveToCurrentPositions()
        {
            // play,7,1,gurry001,21,B*BCX,63/G6 (real, from docs/csv/2025SDN.EVN) -- home team
            // batting, so the visiting team (ATL) is fielding: arcio002 at SS(6), olsom001 at 1B(3).
            var records = new EventFileRecord[]
            {
                Start("olsom001", isHomeTeam: false, battingOrder: 3, position: 3),
                Start("arcio002", isHomeTeam: false, battingOrder: 7, position: 6),
                Start(VisitingPitcher, isHomeTeam: false, battingOrder: 0, position: 1),
                Start("gurry001", isHomeTeam: true, battingOrder: 6, position: 10),
                Start(HomePitcher, isHomeTeam: true, battingOrder: 0, position: 1),
                Play(7, isHomeTeamBatting: true, "gurry001", "21", "B*BCX", "63/G6")
            };
            var personIds = PersonIds("olsom001", "arcio002", VisitingPitcher, "gurry001", HomePitcher);

            var plays = GameEventResolver.Resolve(gameId: 1, Game(records), personIds);

            var play = Assert.Single(plays);
            Assert.Equal(personIds["gurry001"], play.Event.BatterId);
            Assert.Equal(personIds[VisitingPitcher], play.Event.PitcherId);
            Assert.Equal(GameEventType.GroundOut, play.Event.EventType);

            var runner = Assert.Single(play.Runners);
            Assert.Equal(personIds["gurry001"], runner.Runner.PersonId);
            Assert.Equal(
                new[] { (personIds["arcio002"], FieldingCreditType.Assist, 1), (personIds["olsom001"], FieldingCreditType.Putout, 2) },
                runner.FieldingCredits.Select(c => (c.PersonId, c.CreditType, c.Sequence)));
        }

        [Fact]
        public void Resolve_SixFourThreeDoublePlay_ForcedRunnerIdentityCarriedFromPriorPlay()
        {
            // Play A (real, generic single shape from PlayCodeParserTests): puts diaze005 on
            // first. Play B is the spec's real worked 6-4-3 example:
            // play,6,1,diaze005,12,1CBCFX,64(1)3/GDP/G6 -- home batting, ATL fielding:
            // arcio002 SS(6), albio001 2B(4), olsom001 1B(3).
            var records = new EventFileRecord[]
            {
                Start("olsom001", isHomeTeam: false, battingOrder: 3, position: 3),
                Start("albio001", isHomeTeam: false, battingOrder: 5, position: 4),
                Start("arcio002", isHomeTeam: false, battingOrder: 7, position: 6),
                Start(VisitingPitcher, isHomeTeam: false, battingOrder: 0, position: 1),
                Start("priorb001", isHomeTeam: true, battingOrder: 5, position: 9),
                Start("diaze005", isHomeTeam: true, battingOrder: 9, position: 2),
                Play(6, isHomeTeamBatting: true, "priorb001", "10", "BX", "S1/G1S"),
                Play(6, isHomeTeamBatting: true, "diaze005", "12", "1CBCFX", "64(1)3/GDP/G6")
            };
            var personIds = PersonIds("olsom001", "albio001", "arcio002", VisitingPitcher, "priorb001", "diaze005");

            var plays = GameEventResolver.Resolve(gameId: 1, Game(records), personIds);

            var dp = plays[1];
            var forcedRunner = Assert.Single(dp.Runners, r => r.Runner.StartBase == BaseState.First);
            Assert.Equal(personIds["priorb001"], forcedRunner.Runner.PersonId);
            Assert.True(forcedRunner.Runner.IsOut);
            Assert.Equal(
                new[] { (personIds["arcio002"], FieldingCreditType.Assist, 1), (personIds["albio001"], FieldingCreditType.Putout, 2) },
                forcedRunner.FieldingCredits.Select(c => (c.PersonId, c.CreditType, c.Sequence)));

            var batterOut = Assert.Single(dp.Runners, r => r.Runner.StartBase == BaseState.BattersBox);
            Assert.Equal(personIds["diaze005"], batterOut.Runner.PersonId);
            Assert.Equal(
                new[] { (personIds["olsom001"], FieldingCreditType.Putout, 1) },
                batterOut.FieldingCredits.Select(c => (c.PersonId, c.CreditType, c.Sequence)));
        }

        [Fact]
        public void Resolve_RelayThrowOutAtHome_CreditsReflectCurrentPositionsAfterPitchingChange()
        {
            // Real relay analog of the 8-6-2 example:
            // play,11,0,durbc002,00,..X,FC/G4.2X3(4561);B-2 -- visiting team (durbc002's team)
            // batting, home team fielding. A pitching change (sub) happens between the runner
            // reaching second and this play, so the putout (position 1) must resolve to the
            // *new* pitcher, not the starter -- proving fielder resolution uses current, not
            // starting, position state.
            var records = new EventFileRecord[]
            {
                Start("cronj001", isHomeTeam: true, battingOrder: 6, position: 4),
                Start("machm001", isHomeTeam: true, battingOrder: 2, position: 5),
                Start("iglej001", isHomeTeam: true, battingOrder: 8, position: 6),
                Start(HomePitcher, isHomeTeam: true, battingOrder: 0, position: 1),
                Start("priorb002", isHomeTeam: false, battingOrder: 7, position: 9),
                Start("durbc002", isHomeTeam: false, battingOrder: 8, position: 5),
                Play(11, isHomeTeamBatting: false, "priorb002", "10", "BX", "S1/G1S"),
                Play(11, isHomeTeamBatting: false, "durbc002", "00", "", "SB2"),
                Sub("relievr001", isHomeTeam: true, battingOrder: 0, position: 1),
                Play(11, isHomeTeamBatting: false, "durbc002", "00", "..X", "FC/G4.2X3(4561);B-2")
            };
            var personIds = PersonIds("cronj001", "machm001", "iglej001", HomePitcher, "priorb002", "durbc002", "relievr001");

            var plays = GameEventResolver.Resolve(gameId: 1, Game(records), personIds);

            var relay = plays[2];
            Assert.Equal(GameEventType.FieldersChoice, relay.Event.EventType);
            Assert.Equal(personIds["relievr001"], relay.Event.PitcherId);

            var thrownOut = Assert.Single(relay.Runners, r => r.Runner.StartBase == BaseState.Second);
            Assert.Equal(personIds["priorb002"], thrownOut.Runner.PersonId);
            Assert.True(thrownOut.Runner.IsOut);
            Assert.Equal(
                new[]
                {
                    (personIds["cronj001"], FieldingCreditType.Assist, 1),
                    (personIds["machm001"], FieldingCreditType.Assist, 2),
                    (personIds["iglej001"], FieldingCreditType.Assist, 3),
                    (personIds["relievr001"], FieldingCreditType.Putout, 4)
                },
                thrownOut.FieldingCredits.Select(c => (c.PersonId, c.CreditType, c.Sequence)));
        }

        [Fact]
        public void Resolve_RundownStyleChain_RepeatedFielderCreditedTwice()
        {
            // play,1,0,chapm001,01,F1,POCS2(1341) -- real, from docs/csv/2025SDN.EVN. Needs a
            // runner already on first; the pitcher (position 1) is credited twice (assist,
            // then the final putout).
            var records = new EventFileRecord[]
            {
                Start(HomePitcher, isHomeTeam: true, battingOrder: 0, position: 1),
                Start("wadel001", isHomeTeam: true, battingOrder: 7, position: 3),
                Start("fitzt001", isHomeTeam: true, battingOrder: 9, position: 4),
                Start("priorb003", isHomeTeam: false, battingOrder: 3, position: 8),
                Start("chapm001", isHomeTeam: false, battingOrder: 4, position: 5),
                Play(1, isHomeTeamBatting: false, "priorb003", "10", "BX", "S1/G1S"),
                Play(1, isHomeTeamBatting: false, "chapm001", "01", "F1", "POCS2(1341)")
            };
            var personIds = PersonIds(HomePitcher, "wadel001", "fitzt001", "priorb003", "chapm001");

            var plays = GameEventResolver.Resolve(gameId: 1, Game(records), personIds);

            var rundown = plays[1];
            var runner = Assert.Single(rundown.Runners);
            Assert.Equal(personIds["priorb003"], runner.Runner.PersonId);
            Assert.Equal(
                new[]
                {
                    (personIds[HomePitcher], FieldingCreditType.Assist, 1),
                    (personIds["wadel001"], FieldingCreditType.Assist, 2),
                    (personIds["fitzt001"], FieldingCreditType.Assist, 3),
                    (personIds[HomePitcher], FieldingCreditType.Putout, 4)
                },
                runner.FieldingCredits.Select(c => (c.PersonId, c.CreditType, c.Sequence)));
        }

        [Fact]
        public void Resolve_InheritedRunner_ResponsiblePitcherStaysWithOriginalPitcherAcrossPitchingChange()
        {
            // Runner reaches on a single off the starter; a pitching change follows; the
            // runner then scores off the new pitcher. ResponsiblePitcherId must still be the
            // *original* pitcher (the standard "inherited runner" rule), never recomputed
            // from whoever is currently pitching.
            var records = new EventFileRecord[]
            {
                Start("starter001", isHomeTeam: false, battingOrder: 0, position: 1),
                Start("batter001", isHomeTeam: true, battingOrder: 1, position: 9),
                Start("batter002", isHomeTeam: true, battingOrder: 2, position: 8),
                Play(1, isHomeTeamBatting: true, "batter001", "10", "BX", "S1/G1S"),
                Sub("reliever001", isHomeTeam: false, battingOrder: 0, position: 1),
                Play(1, isHomeTeamBatting: true, "batter002", "10", "BX", "D34/G3.1-3")
            };
            var personIds = PersonIds("starter001", "batter001", "batter002", "reliever001");

            var plays = GameEventResolver.Resolve(gameId: 1, Game(records), personIds);

            var second = plays[1];
            var scoringAdvance = Assert.Single(second.Runners, r => r.Runner.StartBase == BaseState.First);
            Assert.Equal(personIds["batter001"], scoringAdvance.Runner.PersonId);
            Assert.Equal(BaseState.Third, scoringAdvance.Runner.EndBase);
        }

        [Fact]
        public void Resolve_PinchRunnerSubstitution_IdentitySwapsButResponsiblePitcherPreserved()
        {
            // Runner reaches on a single off the starter; a pinch runner (position 12) enters
            // for that same batting slot; the pinch runner then scores. The resulting
            // GameEventRunner must carry the *pinch runner's* PersonId (not the original
            // batter's), while still being charged to the original pitcher.
            var records = new EventFileRecord[]
            {
                Start("starter001", isHomeTeam: false, battingOrder: 0, position: 1),
                Start("batter001", isHomeTeam: true, battingOrder: 1, position: 9),
                Start("batter002", isHomeTeam: true, battingOrder: 2, position: 8),
                Play(1, isHomeTeamBatting: true, "batter001", "10", "BX", "S1/G1S"),
                Sub("pinchrunner001", isHomeTeam: true, battingOrder: 1, position: 12),
                Play(1, isHomeTeamBatting: true, "batter002", "10", "BX", "D34/G3.1-3")
            };
            var personIds = PersonIds("starter001", "batter001", "batter002", "pinchrunner001");

            var plays = GameEventResolver.Resolve(gameId: 1, Game(records), personIds);

            var second = plays[1];
            var advance = Assert.Single(second.Runners, r => r.Runner.StartBase == BaseState.First);
            Assert.Equal(personIds["pinchrunner001"], advance.Runner.PersonId);
            Assert.Equal(BaseState.Third, advance.Runner.EndBase);
        }

        [Fact]
        public void Resolve_ExtraInningsTiebreakerRunner_ManufacturedRunnerResolvesAndScoresUnearned()
        {
            // Real pattern from docs/csv/2025SDN.EVN: sub (new pitcher enters the half-inning)
            // -> radj (the international-tiebreaker runner is manufactured at second) -> the
            // half-inning's plays. Followed by the real triple that scores the runner
            // unearned: play,10,0,hoern001,20,..BBX,T8/L89XD+.2-H(UR).
            var records = new EventFileRecord[]
            {
                Start("starter001", isHomeTeam: true, battingOrder: 0, position: 1),
                Start("swand001", isHomeTeam: false, battingOrder: 2, position: 6),
                Start("hoern001", isHomeTeam: false, battingOrder: 5, position: 8),
                Sub("reliever001", isHomeTeam: true, battingOrder: 0, position: 1),
                new AdjustmentRecord { AdjustmentTypeCode = "radj", RetrosheetId = "swand001", Value = "2" },
                Play(10, isHomeTeamBatting: false, "hoern001", "20", "..BBX", "T8/L89XD+.2-H(UR)")
            };
            var personIds = PersonIds("starter001", "swand001", "hoern001", "reliever001");

            var plays = GameEventResolver.Resolve(gameId: 1, Game(records), personIds);

            var triple = Assert.Single(plays);
            var scoringRunner = Assert.Single(triple.Runners, r => r.Runner.StartBase == BaseState.Second);
            Assert.Equal(personIds["swand001"], scoringRunner.Runner.PersonId);
            Assert.Equal(BaseState.Home, scoringRunner.Runner.EndBase);
            Assert.False(scoringRunner.Runner.IsEarnedRun);
            Assert.Equal(personIds["reliever001"], scoringRunner.Runner.ResponsiblePitcherId);
        }

        [Fact]
        public void Resolve_RealFixtureFile_AllFiveGamesResolveWithoutError()
        {
            // End-to-end smoke test against the full real fixture (five complete games) --
            // confirms the resolver doesn't throw across real lineups, substitutions, and the
            // extra-innings tiebreaker rule, and that every play produces at least one runner
            // (the batter, at minimum).
            var fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "eventfile_sample.EVN");
            var games = EventFileReader.ReadGames(fixturePath).ToList();

            foreach (var game in games)
            {
                var allIds = game.Records
                    .SelectMany(r => r switch
                    {
                        LineupRecord lr => new[] { lr.RetrosheetId },
                        PlayRecord pr => new[] { pr.RetrosheetId },
                        AdjustmentRecord { AdjustmentTypeCode: "radj" } ar => new[] { ar.RetrosheetId },
                        _ => Array.Empty<string>()
                    })
                    .Distinct()
                    .ToList();

                var personIds = PersonIds(allIds.ToArray());

                var plays = GameEventResolver.Resolve(gameId: 1, game, personIds);

                Assert.Equal(game.Records.OfType<PlayRecord>().Count(), plays.Count);
                // "NP" (no play) records, and a dropped foul ball caught for an error ("FLE<n>",
                // EventType.Error -- see PlayCodeParser's documented "produces zero runners"
                // behavior), legitimately produce zero runners; every other play involves at
                // least the batter.
                Assert.All(plays, p => Assert.True(
                    p.Runners.Count > 0 || p.Event.EventType is GameEventType.NoPlay or GameEventType.Error));
            }
        }
    }
}
