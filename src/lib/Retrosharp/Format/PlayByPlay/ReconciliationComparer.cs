using System.Collections.Generic;
using System.Linq;

using Retrosharp.Contract.Game;
using Retrosharp.Contract.GameEvent;
using Retrosharp.Contract.Pitching;

namespace Retrosharp.Format.PlayByPlay
{
    /// <summary>
    /// One team-level statistic that disagrees between a play-by-play-derived total and the
    /// Game Log Parser's persisted Game*Statistics row for the same game/franchise. A null
    /// <see cref="PersistedValue"/> means no Game*Statistics row exists at all for this
    /// franchise/game/<see cref="StatisticGroup"/> -- a data gap, not a value mismatch -- in
    /// which case <see cref="Field"/> is <see cref="ReconciliationComparer.NoPersistedRowField"/>.
    /// </summary>
    public sealed record TeamStatisticsDiscrepancy(int FranchiseId, string StatisticGroup, string Field, short? PersistedValue, short DerivedValue);

    /// <summary>
    /// One pitcher whose Pitching.EarnedRuns (sourced from the event file's own "data,er,..."
    /// record by Step 6d) disagrees with the independently-computed earned-run figure derived
    /// from play-by-play.
    /// </summary>
    public sealed record EarnedRunDiscrepancy(int PersonId, short DataRecordEarnedRuns, short IndependentlyComputedEarnedRuns);

    /// <summary>
    /// Compares play-by-play-derived totals against already-persisted data from other parsers,
    /// producing a plain list of discrepancies -- no logging, no I/O. Pure logic, so the step's
    /// Definition of Done ("an intentionally-introduced discrepancy... produces a logged
    /// warning") is directly unit-testable here, before the caller ever touches ILogger. See
    /// spec/phase-1-build-plan.md Step 6e.
    /// </summary>
    public static class ReconciliationComparer
    {
        public const string NoPersistedRowField = "<no persisted row>";

        public static IReadOnlyList<TeamStatisticsDiscrepancy> CompareTeamTotals(
            GameTeamStatisticsDelta derived,
            IEnumerable<GameBattingStatistics> persistedBatting,
            IEnumerable<GamePitchingStatistics> persistedPitching,
            IEnumerable<GameFieldingStatistics> persistedFielding)
        {
            var discrepancies = new List<TeamStatisticsDiscrepancy>();

            var battingByFranchise = persistedBatting.ToDictionary(b => b.FranchiseId);
            foreach (var team in derived.Battings)
            {
                if (!battingByFranchise.TryGetValue(team.FranchiseId, out var persisted))
                {
                    discrepancies.Add(new TeamStatisticsDiscrepancy(team.FranchiseId, "Batting", NoPersistedRowField, null, 0));
                    continue;
                }

                CompareField(discrepancies, team.FranchiseId, "Batting", nameof(team.PlateAppearances), persisted.PlateAppearances, team.PlateAppearances);
                CompareField(discrepancies, team.FranchiseId, "Batting", nameof(team.AtBats), persisted.AtBats, team.AtBats);
                CompareField(discrepancies, team.FranchiseId, "Batting", nameof(team.Hits), persisted.Hit, team.Hits);
                CompareField(discrepancies, team.FranchiseId, "Batting", nameof(team.Doubles), persisted.Doubles, team.Doubles);
                CompareField(discrepancies, team.FranchiseId, "Batting", nameof(team.Triples), persisted.Triples, team.Triples);
                CompareField(discrepancies, team.FranchiseId, "Batting", nameof(team.Homeruns), persisted.Homeruns, team.Homeruns);
                CompareField(discrepancies, team.FranchiseId, "Batting", nameof(team.RunsBattedIn), persisted.RunsBattedIn, team.RunsBattedIn);
                CompareField(discrepancies, team.FranchiseId, "Batting", nameof(team.BaseOnBalls), persisted.BaseOnBalls, team.BaseOnBalls);
                CompareField(discrepancies, team.FranchiseId, "Batting", nameof(team.Strikeouts), persisted.Strikeouts, team.Strikeouts);
                CompareField(discrepancies, team.FranchiseId, "Batting", nameof(team.SacrificeFlies), persisted.SacrificeFlies, team.SacrificeFlies);
                CompareField(discrepancies, team.FranchiseId, "Batting", nameof(team.SacrificeBunts), persisted.SacrificeBunts, team.SacrificeBunts);
                CompareField(discrepancies, team.FranchiseId, "Batting", nameof(team.IntentionalBb), persisted.IntentionalBb, team.IntentionalBb);
                CompareField(discrepancies, team.FranchiseId, "Batting", nameof(team.HitByPitches), persisted.HitByPitches, team.HitByPitches);
                CompareField(discrepancies, team.FranchiseId, "Batting", nameof(team.StolenBases), persisted.StolenBases, team.StolenBases);
                CompareField(discrepancies, team.FranchiseId, "Batting", nameof(team.TimesCaughtStealing), persisted.TimesCaughtStealing, team.TimesCaughtStealing);
                CompareField(discrepancies, team.FranchiseId, "Batting", nameof(team.Runs), persisted.Runs, team.Runs);
                CompareField(discrepancies, team.FranchiseId, "Batting", nameof(team.GroundedIntoDoublePlay), persisted.GroundedIntoDoublePlay, team.GroundedIntoDoublePlay);
            }

            var pitchingByFranchise = persistedPitching.ToDictionary(p => p.FranchiseId);
            foreach (var team in derived.Pitchings)
            {
                if (!pitchingByFranchise.TryGetValue(team.FranchiseId, out var persisted))
                {
                    discrepancies.Add(new TeamStatisticsDiscrepancy(team.FranchiseId, "Pitching", NoPersistedRowField, null, 0));
                    continue;
                }

                CompareField(discrepancies, team.FranchiseId, "Pitching", nameof(team.PitchersUsed), persisted.PitchersUsed, team.PitchersUsed);
                CompareField(discrepancies, team.FranchiseId, "Pitching", nameof(team.IndividualEarnedRuns), persisted.IndividualEarnedRuns, team.IndividualEarnedRuns);
                CompareField(discrepancies, team.FranchiseId, "Pitching", nameof(team.WildPitches), persisted.WildPitches, team.WildPitches);
                CompareField(discrepancies, team.FranchiseId, "Pitching", nameof(team.Balks), persisted.Balks, team.Balks);
            }

            var fieldingByFranchise = persistedFielding.ToDictionary(f => f.FranchiseId);
            foreach (var team in derived.Fieldings)
            {
                if (!fieldingByFranchise.TryGetValue(team.FranchiseId, out var persisted))
                {
                    discrepancies.Add(new TeamStatisticsDiscrepancy(team.FranchiseId, "Fielding", NoPersistedRowField, null, 0));
                    continue;
                }

                CompareField(discrepancies, team.FranchiseId, "Fielding", nameof(team.Putouts), persisted.Putouts, team.Putouts);
                CompareField(discrepancies, team.FranchiseId, "Fielding", nameof(team.Assists), persisted.Assists, team.Assists);
                CompareField(discrepancies, team.FranchiseId, "Fielding", nameof(team.Errors), persisted.Errors, team.Errors);
            }

            return discrepancies;
        }

        public static IReadOnlyList<EarnedRunDiscrepancy> CompareEarnedRuns(
            IReadOnlyList<PitchingDelta> pitchingDeltas,
            IReadOnlyDictionary<int, short> independentEarnedRuns)
        {
            var discrepancies = new List<EarnedRunDiscrepancy>();

            foreach (var pitching in pitchingDeltas)
            {
                var independentValue = independentEarnedRuns.GetValueOrDefault(pitching.PersonId);
                if (independentValue != pitching.EarnedRuns)
                    discrepancies.Add(new EarnedRunDiscrepancy(pitching.PersonId, pitching.EarnedRuns, independentValue));
            }

            return discrepancies;
        }

        private static void CompareField(List<TeamStatisticsDiscrepancy> discrepancies, int franchiseId, string statisticGroup, string field, short persistedValue, short derivedValue)
        {
            if (persistedValue != derivedValue)
                discrepancies.Add(new TeamStatisticsDiscrepancy(franchiseId, statisticGroup, field, persistedValue, derivedValue));
        }
    }
}
