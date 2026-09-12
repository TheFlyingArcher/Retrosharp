using Retrosharp.Contract.GameEvent;
using Retrosharp.Format.EventFile;

namespace Retrosharp.Format.Tests
{
    /// <summary>
    /// Exercises <see cref="GameContextResolver"/> against hand-assembled <see cref="EventFileGame"/>
    /// objects built from real record lines (mirroring <see cref="GameEventResolverTests"/>'s
    /// convention) -- unlike the play-by-play resolver, this needs no baserunner/lineup state,
    /// so each test is a direct mapping check.
    /// </summary>
    public class GameContextResolverTests
    {
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
        public void Resolve_StartRecordsExcludedFromSubstitutions_OnlySubRecordsPersisted()
        {
            // start,profj001,"Jurickson Profar",0,1,7 (real, docs/csv/2025SDN.EVN) -- a starting
            // lineup slot, not a substitution -- must not appear in the output.
            // sub,jacoa001,"Alek Jacob",1,0,1 (real) -- a genuine mid-game substitution.
            var records = new EventFileRecord[]
            {
                new StartRecord { RetrosheetId = "profj001", Name = "Jurickson Profar", IsHomeTeam = false, BattingOrder = 1, Position = 7 },
                new SubRecord { RetrosheetId = "jacoa001", Name = "Alek Jacob", IsHomeTeam = true, BattingOrder = 0, Position = 1 }
            };
            var personIds = new Dictionary<string, int> { ["profj001"] = 1, ["jacoa001"] = 2 };

            var (substitutions, _, _) = GameContextResolver.Resolve(gameId: 42, Game(records), personIds);

            var substitution = Assert.Single(substitutions);
            Assert.Equal(42, substitution.GameId);
            Assert.Equal(1, substitution.Sequence);
            Assert.Equal(2, substitution.PersonId);
            Assert.Equal("H", substitution.TeamAtBat);
            Assert.Equal(0, substitution.BattingOrderPosition);
            Assert.Equal(1, substitution.FieldingPosition);
        }

        [Fact]
        public void Resolve_MultipleSubstitutions_SequenceIncrementsIndependently()
        {
            var records = new EventFileRecord[]
            {
                new SubRecord { RetrosheetId = "jacoa001", Name = "Alek Jacob", IsHomeTeam = true, BattingOrder = 0, Position = 1 },
                new SubRecord { RetrosheetId = "matsy001", Name = "Yuki Matsui", IsHomeTeam = true, BattingOrder = 0, Position = 1 },
                new SubRecord { RetrosheetId = "sheeg001", Name = "Gavin Sheets", IsHomeTeam = true, BattingOrder = 8, Position = 11 }
            };
            var personIds = new Dictionary<string, int> { ["jacoa001"] = 1, ["matsy001"] = 2, ["sheeg001"] = 3 };

            var (substitutions, _, _) = GameContextResolver.Resolve(gameId: 1, Game(records), personIds);

            Assert.Equal(new[] { 1, 2, 3 }, substitutions.Select(s => s.Sequence));
            Assert.Equal(new[] { 1, 2, 3 }, substitutions.Select(s => s.PersonId));
        }

        [Theory]
        [InlineData("badj", GameAdjustmentType.BattingHandedness)]
        [InlineData("padj", GameAdjustmentType.PitchingHandedness)]
        [InlineData("radj", GameAdjustmentType.RunnerPlacement)]
        [InlineData("presadj", GameAdjustmentType.PitcherResponsibility)]
        public void Resolve_EveryAdjustmentTypeCode_MapsToCorrectEnumValue(string adjustmentTypeCode, GameAdjustmentType expected)
        {
            // badj,seiga001,R (real, docs/csv/2025SEA.EVA) is the only one of these with a real
            // example in either reference file; the rest are exercised synthetically against
            // the same shape, since padj/presadj don't occur in either file. "ladj" is excluded
            // here -- its first field is the batting team, not a player id, so it has its own
            // tests below.
            var records = new EventFileRecord[]
            {
                new AdjustmentRecord { AdjustmentTypeCode = adjustmentTypeCode, RetrosheetId = "seiga001", Value = "R" }
            };
            var personIds = new Dictionary<string, int> { ["seiga001"] = 7 };

            var (_, adjustments, _) = GameContextResolver.Resolve(gameId: 1, Game(records), personIds);

            var adjustment = Assert.Single(adjustments);
            Assert.Equal(expected, adjustment.AdjustmentType);
            Assert.Equal(7, adjustment.PersonId);
            Assert.Equal("R", adjustment.Value);
            Assert.Equal(1, adjustment.Sequence);
        }

        [Fact]
        public void Resolve_LadjBattingOutOfOrder_ResolvesToPlayerInNamedSlotOfNamedTeam()
        {
            // ladj,0,4 -- the visiting team's 4th batting-order slot is due up. The record
            // names the slot, not the player, so the resolver has to recover the batter from
            // whoever the starting lineup put in that slot. A home-team slot 4 with a different
            // player is present to prove the team field (0 = visitor) is honoured.
            var records = new EventFileRecord[]
            {
                new StartRecord { RetrosheetId = "vis4th001", Name = "Visitor Four", IsHomeTeam = false, BattingOrder = 4, Position = 5 },
                new StartRecord { RetrosheetId = "home4th001", Name = "Home Four", IsHomeTeam = true, BattingOrder = 4, Position = 5 },
                new AdjustmentRecord { AdjustmentTypeCode = "ladj", RetrosheetId = "0", Value = "4" }
            };
            var personIds = new Dictionary<string, int> { ["vis4th001"] = 44, ["home4th001"] = 99 };

            var (_, adjustments, _) = GameContextResolver.Resolve(gameId: 1, Game(records), personIds);

            var adjustment = Assert.Single(adjustments);
            Assert.Equal(GameAdjustmentType.LineupPosition, adjustment.AdjustmentType);
            Assert.Equal(44, adjustment.PersonId);
            Assert.Equal("4", adjustment.Value);
        }

        [Fact]
        public void Resolve_LadjAfterSubstitutionIntoSlot_ResolvesToCurrentOccupant()
        {
            // A pinch hitter takes over slot 6, then that slot bats out of order (ladj,1,6 --
            // home team). The adjustment must resolve to the substitute, not the starter.
            var records = new EventFileRecord[]
            {
                new StartRecord { RetrosheetId = "starter006", Name = "Starter Six", IsHomeTeam = true, BattingOrder = 6, Position = 7 },
                new SubRecord { RetrosheetId = "pinch006", Name = "Pinch Six", IsHomeTeam = true, BattingOrder = 6, Position = 11 },
                new AdjustmentRecord { AdjustmentTypeCode = "ladj", RetrosheetId = "1", Value = "6" }
            };
            var personIds = new Dictionary<string, int> { ["starter006"] = 6, ["pinch006"] = 61 };

            var (_, adjustments, _) = GameContextResolver.Resolve(gameId: 1, Game(records), personIds);

            Assert.Equal(61, Assert.Single(adjustments).PersonId);
        }

        [Fact]
        public void Resolve_LadjForSlotNeverFilled_ThrowsRatherThanGuessing()
        {
            var records = new EventFileRecord[]
            {
                new AdjustmentRecord { AdjustmentTypeCode = "ladj", RetrosheetId = "0", Value = "4" }
            };

            Assert.Throws<InvalidOperationException>(() =>
                GameContextResolver.Resolve(gameId: 1, Game(records), new Dictionary<string, int>()));
        }

        [Fact]
        public void Resolve_RadjRunnerPlacement_PreservesBaseValueAndPersonId()
        {
            // radj,swand001,2 (real, docs/csv/2025SDN.EVN) -- extra-innings tiebreaker runner
            // placement at second base.
            var records = new EventFileRecord[]
            {
                new AdjustmentRecord { AdjustmentTypeCode = "radj", RetrosheetId = "swand001", Value = "2" }
            };
            var personIds = new Dictionary<string, int> { ["swand001"] = 5 };

            var (_, adjustments, _) = GameContextResolver.Resolve(gameId: 1, Game(records), personIds);

            var adjustment = Assert.Single(adjustments);
            Assert.Equal(GameAdjustmentType.RunnerPlacement, adjustment.AdjustmentType);
            Assert.Equal(5, adjustment.PersonId);
            Assert.Equal("2", adjustment.Value);
        }

        [Fact]
        public void Resolve_CommentRecords_TextPreservedNoPersonIdNeeded()
        {
            // com,"$Braves challenged (play at 1st), call on the field was overturned."
            // (real, docs/csv/2025SDN.EVN -- the same embedded-comma comment already validated
            // for quote-aware parsing by EventFileReaderTests).
            var records = new EventFileRecord[]
            {
                new ComRecord { CommentText = "$Braves challenged (play at 1st), call on the field was overturned." }
            };

            var (_, _, comments) = GameContextResolver.Resolve(gameId: 9, Game(records), new Dictionary<string, int>());

            var comment = Assert.Single(comments);
            Assert.Equal(9, comment.GameId);
            Assert.Equal(1, comment.Sequence);
            Assert.Equal("$Braves challenged (play at 1st), call on the field was overturned.", comment.CommentText);
        }

        [Fact]
        public void Resolve_UnresolvablePersonId_ThrowsRatherThanGuessing()
        {
            var records = new EventFileRecord[]
            {
                new SubRecord { RetrosheetId = "unknown001", Name = "Nobody", IsHomeTeam = true, BattingOrder = 0, Position = 1 }
            };

            Assert.Throws<InvalidOperationException>(() =>
                GameContextResolver.Resolve(gameId: 1, Game(records), new Dictionary<string, int>()));
        }
    }
}
