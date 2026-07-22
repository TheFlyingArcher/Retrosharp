using Retrosharp.Contract.Game;
using Retrosharp.Contract.GameEvent;
using Retrosharp.Contract.Pitching;
using Retrosharp.Format.PlayByPlay;

namespace Retrosharp.Format.Tests
{
    /// <summary>
    /// Exercises <see cref="ReconciliationComparer"/> against hand-built derived/persisted
    /// pairs -- pure comparison logic, no I/O. This is what makes Step 6e's own Definition of
    /// Done ("an intentionally-introduced discrepancy... produces a logged warning") directly
    /// testable: feed a deliberately mismatched pair, assert the discrepancy list is exactly
    /// right, before ILogger ever enters the picture. See spec/phase-1-build-plan.md Step 6e.
    /// </summary>
    public class ReconciliationComparerTests
    {
        private const int FranchiseId = 1;

        private static TeamBattingTotal Batting(short hits = 0, short plateAppearances = 0) =>
            new() { FranchiseId = FranchiseId, Hits = hits, PlateAppearances = plateAppearances };

        private static GameBattingStatistics PersistedBatting(short hit = 0, short plateAppearances = 0) =>
            new() { FranchiseId = FranchiseId, Hit = hit, PlateAppearances = plateAppearances };

        [Fact]
        public void CompareTeamTotals_PerfectMatch_ProducesNoDiscrepancies()
        {
            var derived = new GameTeamStatisticsDelta
            {
                Battings = [Batting(hits: 2, plateAppearances: 4)],
                Pitchings = [],
                Fieldings = []
            };
            var persistedBatting = new[] { PersistedBatting(hit: 2, plateAppearances: 4) };

            var discrepancies = ReconciliationComparer.CompareTeamTotals(derived, persistedBatting, [], []);

            Assert.Empty(discrepancies);
        }

        [Fact]
        public void CompareTeamTotals_MismatchedHits_ProducesExactlyOneDiscrepancyWithCorrectValues()
        {
            var derived = new GameTeamStatisticsDelta
            {
                Battings = [Batting(hits: 5, plateAppearances: 4)],
                Pitchings = [],
                Fieldings = []
            };
            // Game Log Parser recorded 6 hits for this team/game -- the intentionally-introduced
            // discrepancy this step's Definition of Done calls for.
            var persistedBatting = new[] { PersistedBatting(hit: 6, plateAppearances: 4) };

            var discrepancies = ReconciliationComparer.CompareTeamTotals(derived, persistedBatting, [], []);

            var discrepancy = Assert.Single(discrepancies);
            Assert.Equal(FranchiseId, discrepancy.FranchiseId);
            Assert.Equal("Batting", discrepancy.StatisticGroup);
            Assert.Equal("Hits", discrepancy.Field);
            Assert.Equal((short?)6, discrepancy.PersistedValue);
            Assert.Equal(5, discrepancy.DerivedValue);
        }

        [Fact]
        public void CompareTeamTotals_NoPersistedRow_ProducesOneMissingRowDiscrepancy()
        {
            var derived = new GameTeamStatisticsDelta
            {
                Battings = [Batting(hits: 1)],
                Pitchings = [],
                Fieldings = []
            };

            var discrepancies = ReconciliationComparer.CompareTeamTotals(derived, [], [], []);

            var discrepancy = Assert.Single(discrepancies);
            Assert.Equal("Batting", discrepancy.StatisticGroup);
            Assert.Equal(ReconciliationComparer.NoPersistedRowField, discrepancy.Field);
            Assert.Null(discrepancy.PersistedValue);
        }

        [Fact]
        public void CompareEarnedRuns_Match_ProducesNoDiscrepancies()
        {
            var pitchingDeltas = new[] { new PitchingDelta { PersonId = 50, FranchiseId = FranchiseId, SeasonYear = 2025, EarnedRuns = 3 } };
            var independentEarnedRuns = new Dictionary<int, short> { [50] = 3 };

            var discrepancies = ReconciliationComparer.CompareEarnedRuns(pitchingDeltas, independentEarnedRuns);

            Assert.Empty(discrepancies);
        }

        [Fact]
        public void CompareEarnedRuns_Mismatch_ProducesDiscrepancyWithBothValues()
        {
            // Pitching.EarnedRuns (sourced from "data,er,...") says 3, but play-by-play's own
            // IsEarnedRun annotations independently derive only 2.
            var pitchingDeltas = new[] { new PitchingDelta { PersonId = 50, FranchiseId = FranchiseId, SeasonYear = 2025, EarnedRuns = 3 } };
            var independentEarnedRuns = new Dictionary<int, short> { [50] = 2 };

            var discrepancies = ReconciliationComparer.CompareEarnedRuns(pitchingDeltas, independentEarnedRuns);

            var discrepancy = Assert.Single(discrepancies);
            Assert.Equal(50, discrepancy.PersonId);
            Assert.Equal(3, discrepancy.DataRecordEarnedRuns);
            Assert.Equal(2, discrepancy.IndependentlyComputedEarnedRuns);
        }

        [Fact]
        public void CompareEarnedRuns_PitcherMissingFromIndependentComputation_DefaultsToZero()
        {
            var pitchingDeltas = new[] { new PitchingDelta { PersonId = 51, FranchiseId = FranchiseId, SeasonYear = 2025, EarnedRuns = 1 } };

            var discrepancies = ReconciliationComparer.CompareEarnedRuns(pitchingDeltas, new Dictionary<int, short>());

            var discrepancy = Assert.Single(discrepancies);
            Assert.Equal(1, discrepancy.DataRecordEarnedRuns);
            Assert.Equal(0, discrepancy.IndependentlyComputedEarnedRuns);
        }
    }
}
