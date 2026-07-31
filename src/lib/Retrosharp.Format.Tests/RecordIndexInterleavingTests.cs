using Retrosharp.Format.EventFile;
using Retrosharp.Format.PlayByPlay;

namespace Retrosharp.Format.Tests
{
    /// <summary>
    /// Proves the fix for a real gap found while planning Step 7f (spec/phase-1-build-plan.md):
    /// <see cref="GameEvent"/>/<see cref="Retrosharp.Contract.GameEvent.GameSubstitution"/>/
    /// <see cref="Retrosharp.Contract.GameEvent.GameAdjustment"/>/
    /// <see cref="Retrosharp.Contract.GameEvent.GameComment"/>'s existing <c>Sequence</c> field is
    /// a per-type counter, not a shared ordinal -- <see cref="GameEventResolver"/> and
    /// <see cref="GameContextResolver"/> each walk the same <see cref="EventFileGame.Records"/>
    /// list but only increment their own type's counter, so a play's <c>Sequence</c> and a
    /// substitution's <c>Sequence</c> have no defined relationship to each other. <c>RecordIndex</c>
    /// (the shared position within <c>Records</c> that both resolvers now stamp) is what actually
    /// lets a display layer interleave all four record types in true chronological order.
    /// </summary>
    public class RecordIndexInterleavingTests
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
        public void Resolve_MixedRecordTypes_RecordIndexReflectsTrueFileOrderAcrossBothResolvers()
        {
            // Records[0..4] establish lineups (same shape already verified by
            // GameEventResolverTests). Records[5..9] interleave a comment, the real
            // 63/G6 ground out (index 6, from docs/csv/2025SDN.EVN), an adjustment, a
            // substitution, and a second comment -- proving RecordIndex (not each type's own
            // Sequence) reconstructs their true relative order.
            var records = new EventFileRecord[]
            {
                Start("olsom001", isHomeTeam: false, battingOrder: 3, position: 3),
                Start("arcio002", isHomeTeam: false, battingOrder: 7, position: 6),
                Start(VisitingPitcher, isHomeTeam: false, battingOrder: 0, position: 1),
                Start("gurry001", isHomeTeam: true, battingOrder: 6, position: 10),
                Start(HomePitcher, isHomeTeam: true, battingOrder: 0, position: 1),
                new ComRecord { CommentText = "Before the play." },
                new PlayRecord { Inning = 7, IsHomeTeamBatting = true, RetrosheetId = "gurry001", CountField = "21", PitchSequence = "B*BCX", RawEventText = "63/G6" },
                new AdjustmentRecord { AdjustmentTypeCode = "badj", RetrosheetId = "gurry001", Value = "R" },
                new SubRecord { RetrosheetId = "sheeg001", Name = "Gavin Sheets", IsHomeTeam = true, BattingOrder = 8, Position = 11 },
                new ComRecord { CommentText = "After the sub." }
            };
            var personIds = new Dictionary<string, int>(
                PersonIds("olsom001", "arcio002", VisitingPitcher, "gurry001", HomePitcher))
            {
                ["sheeg001"] = 100
            };

            var plays = GameEventResolver.Resolve(gameId: 1, Game(records), personIds);
            var (substitutions, adjustments, comments) = GameContextResolver.Resolve(gameId: 1, Game(records), personIds);

            var play = Assert.Single(plays);
            var substitution = Assert.Single(substitutions);
            var adjustment = Assert.Single(adjustments);
            Assert.Equal(2, comments.Count);

            Assert.Equal(5, comments[0].RecordIndex);
            Assert.Equal(6, play.Event.RecordIndex);
            Assert.Equal(7, adjustment.RecordIndex);
            Assert.Equal(8, substitution.RecordIndex);
            Assert.Equal(9, comments[1].RecordIndex);

            // The actual acceptance criterion: interleaving by RecordIndex alone (not by each
            // type's own, unrelated Sequence value) reconstructs the original file order.
            var interleaved = new[]
            {
                (RecordIndex: comments[0].RecordIndex, Kind: "comment"),
                (RecordIndex: play.Event.RecordIndex, Kind: "play"),
                (RecordIndex: adjustment.RecordIndex, Kind: "adjustment"),
                (RecordIndex: substitution.RecordIndex, Kind: "substitution"),
                (RecordIndex: comments[1].RecordIndex, Kind: "comment")
            };
            Assert.Equal(
                new[] { "comment", "play", "adjustment", "substitution", "comment" },
                interleaved.OrderBy(x => x.RecordIndex).Select(x => x.Kind));
        }
    }
}
