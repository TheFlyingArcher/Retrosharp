using Retrosharp.Contract.Game;
using Retrosharp.Contract.GameEvent;
using Retrosharp.Service;

namespace Retrosharp.Service.Tests
{
    /// <summary>
    /// Constructed-fixture tests for <see cref="GameSummaryService.ResolvePosition"/> (Step 7i,
    /// see spec/phase-1-build-plan.md) -- a pure function, so no fakes/DB needed.
    /// </summary>
    public class GameSummaryPositionResolutionTests
    {
        private const int PersonId = 42;
        private const string HomeVisitor = "H";

        private static GameLineup LineupEntry(int batterId, string homeVisitor, string position) => new()
        {
            BatterId = batterId,
            HomeVisitor = homeVisitor,
            Position = position
        };

        private static GameSubstitution Substitution(int personId, string teamAtBat, byte fieldingPosition) => new()
        {
            PersonId = personId,
            TeamAtBat = teamAtBat,
            FieldingPosition = fieldingPosition
        };

        [Fact]
        public void StartingLineupOnly_ReturnsSinglePosition()
        {
            var lineups = new List<GameLineup> { LineupEntry(PersonId, HomeVisitor, "7") };
            var subs = new List<GameSubstitution>();

            var position = GameSummaryService.ResolvePosition(PersonId, HomeVisitor, lineups, subs);

            Assert.Equal("7", position);
        }

        [Fact]
        public void SubstituteWithNoStartingLineupEntry_ReturnsSubstitutionPosition()
        {
            var lineups = new List<GameLineup>();
            var subs = new List<GameSubstitution> { Substitution(PersonId, HomeVisitor, 9) };

            var position = GameSummaryService.ResolvePosition(PersonId, HomeVisitor, lineups, subs);

            Assert.Equal("9", position);
        }

        [Fact]
        public void StarterWhoChangesPositionMidGame_ReturnsBothPositionsInOrder()
        {
            // Started in left field (7), moved to center field (8) later in the game -- a real
            // Retrosheet event file records this as another `sub` record for the same PersonId.
            var lineups = new List<GameLineup> { LineupEntry(PersonId, HomeVisitor, "7") };
            var subs = new List<GameSubstitution> { Substitution(PersonId, HomeVisitor, 8) };

            var position = GameSummaryService.ResolvePosition(PersonId, HomeVisitor, lineups, subs);

            Assert.Equal("7,8", position);
        }

        [Fact]
        public void DuplicatePositionAcrossRecords_IsNotRepeated()
        {
            var lineups = new List<GameLineup> { LineupEntry(PersonId, HomeVisitor, "7") };
            // A substitution record re-affirming the same position (e.g. a pinch runner event
            // elsewhere in the file touching this GameId/PersonId combination) shouldn't produce
            // "7,7".
            var subs = new List<GameSubstitution> { Substitution(PersonId, HomeVisitor, 7) };

            var position = GameSummaryService.ResolvePosition(PersonId, HomeVisitor, lineups, subs);

            Assert.Equal("7", position);
        }

        [Fact]
        public void NoMatchingRecordOnEitherSide_ReturnsNull()
        {
            var lineups = new List<GameLineup> { LineupEntry(999, HomeVisitor, "7") };
            var subs = new List<GameSubstitution> { Substitution(999, HomeVisitor, 8) };

            var position = GameSummaryService.ResolvePosition(PersonId, HomeVisitor, lineups, subs);

            Assert.Null(position);
        }

        [Fact]
        public void OpponentSideRecordsAreIgnored_EvenWithMatchingPersonId()
        {
            // Guards against the same class of home/visitor mix-up bug the manager-history
            // collapsing logic (Step 7g) had to guard against.
            var lineups = new List<GameLineup> { LineupEntry(PersonId, "V", "7") };
            var subs = new List<GameSubstitution> { Substitution(PersonId, "V", 8) };

            var position = GameSummaryService.ResolvePosition(PersonId, HomeVisitor, lineups, subs);

            Assert.Null(position);
        }
    }
}
