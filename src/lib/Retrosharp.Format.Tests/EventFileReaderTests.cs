using Retrosharp.Format.EventFile;

namespace Retrosharp.Format.Tests
{
    /// <summary>
    /// Exercises <see cref="EventFileReader"/> against Fixtures/eventfile_sample.EVN -- five
    /// real, complete games trimmed from docs/csv/2025SDN.EVN (id records: SDN202503270,
    /// SDN202503300, SDN202504150, SDN202504300, SDN202509220), chosen because they contain
    /// real instances of every worked example in spec/game-event.md's Data Model section, a
    /// real extra-innings tiebreaker-rule runner placement ("radj"), and a real pinch-runner
    /// substitution.
    /// </summary>
    public class EventFileReaderTests
    {
        private static string FixturePath => Path.Combine(AppContext.BaseDirectory, "Fixtures", "eventfile_sample.EVN");

        [Fact]
        public void ReadGames_FiveRealGames_ReturnsExactlyFiveInFileOrder()
        {
            var games = EventFileReader.ReadGames(FixturePath).ToList();

            Assert.Equal(
                new[] { "SDN202503270", "SDN202503300", "SDN202504150", "SDN202504300", "SDN202509220" },
                games.Select(g => g.GameId));
        }

        [Fact]
        public void ReadGames_FirstGame_InfoFieldsResolvedCorrectly()
        {
            // id,SDN202503270 / info,visteam,ATL / info,hometeam,SDN / info,date,2025/03/27 / info,number,0
            var game = EventFileReader.ReadGames(FixturePath).First();

            Assert.Equal("ATL", game.VisitingTeamCode);
            Assert.Equal("SDN", game.HomeTeamCode);
            Assert.Equal(new DateTime(2025, 3, 27), game.GameDate);
            Assert.Equal(0, game.GameNumber);
        }

        [Theory]
        [InlineData("SDN202503270", 97, 20, 0)]
        [InlineData("SDN202503300", 76, 20, 0)]
        [InlineData("SDN202504150", 105, 20, 2)] // the first two of the fixture's six radj placements
        [InlineData("SDN202504300", 80, 20, 0)]
        [InlineData("SDN202509220", 116, 20, 4)] // the remaining four radj placements
        public void ReadGames_EachGame_RecordCountsMatchRealFile(string gameId, int expectedPlays, int expectedStartRecords, int expectedRadjRecords)
        {
            var game = EventFileReader.ReadGames(FixturePath).Single(g => g.GameId == gameId);

            Assert.Equal(expectedPlays, game.Records.OfType<PlayRecord>().Count());
            Assert.Equal(expectedStartRecords, game.Records.OfType<StartRecord>().Count());
            Assert.Equal(expectedRadjRecords, game.Records.OfType<AdjustmentRecord>().Count(r => r.AdjustmentTypeCode == "radj"));
        }

        [Fact]
        public void ReadGames_ComRecordWithEmbeddedComma_ParsesAsOneFieldNotSplit()
        {
            // com,"$Braves challenged (play at 1st), call on the field was overturned."
            // A naive string.Split(',') would break this into two records; CsvHelper's
            // quote-aware tokenization must keep it as one CommentText field, and parsing
            // must continue correctly into the following "play" record.
            var game = EventFileReader.ReadGames(FixturePath).Single(g => g.GameId == "SDN202503270");

            var comment = Assert.Single(game.Records.OfType<ComRecord>(),
                c => c.CommentText.StartsWith("$Braves challenged"));
            Assert.Equal("$Braves challenged (play at 1st), call on the field was overturned.", comment.CommentText);

            // The record immediately after it in file order is still a well-formed play,
            // proving the embedded comma didn't desynchronize subsequent parsing.
            var commentIndex = game.Records.TakeWhile(r => r != comment).Count();
            var next = Assert.IsType<PlayRecord>(game.Records[commentIndex + 1]);
            Assert.Equal("harrm004", next.RetrosheetId);
            Assert.Equal("WP.1-2", next.RawEventText);
        }

        [Fact]
        public void ReadGames_RadjRecord_CapturesRunnerPlacementFields()
        {
            // sub,matsy001,"Yuki Matsui",1,0,1 / radj,swand001,2 / com,"Dansby Swanson starts inning at 2nd base."
            var game = EventFileReader.ReadGames(FixturePath).Single(g => g.GameId == "SDN202504150");

            var radj = Assert.Single(game.Records.OfType<AdjustmentRecord>(), r => r.AdjustmentTypeCode == "radj" && r.RetrosheetId == "swand001");
            Assert.Equal("2", radj.Value);

            // Confirms the "sub -> radj -> com" ordering this scenario always follows in the
            // real data is preserved: the manufactured runner is placed only after the new
            // half-inning's pitcher has already entered.
            var radjIndex = game.Records.TakeWhile(r => r != radj).Count();
            var precedingSub = Assert.IsType<SubRecord>(game.Records[radjIndex - 1]);
            Assert.Equal("matsy001", precedingSub.RetrosheetId);
            Assert.Equal(1, precedingSub.Position);
        }

        [Fact]
        public void ReadGames_PinchRunnerSubstitution_ParsedWithPosition12()
        {
            // sub,perkb002,"Blake Perkins",0,6,12 -- a real pinch-runner entering for the
            // player batting 6th for the visiting team.
            var game = EventFileReader.ReadGames(FixturePath).Single(g => g.GameId == "SDN202509220");

            var pinchRunner = Assert.Single(game.Records.OfType<SubRecord>(),
                s => s.RetrosheetId == "perkb002" && s.Position == 12);

            Assert.False(pinchRunner.IsHomeTeam);
            Assert.Equal(6, pinchRunner.BattingOrder);
        }

        [Fact]
        public void ReadGames_PlayRecord_FieldsSplitCorrectly()
        {
            // play,7,1,gurry001,21,B*BCX,63/G6
            var game = EventFileReader.ReadGames(FixturePath).Single(g => g.GameId == "SDN202503270");

            var play = Assert.Single(game.Records.OfType<PlayRecord>(),
                p => p.RetrosheetId == "gurry001" && p.RawEventText == "63/G6");

            Assert.Equal(7, play.Inning);
            Assert.True(play.IsHomeTeamBatting);
            Assert.Equal("21", play.CountField);
            Assert.Equal("B*BCX", play.PitchSequence);
        }
    }
}
