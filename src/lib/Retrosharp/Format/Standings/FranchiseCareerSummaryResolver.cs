using System.Collections.Generic;
using System.Linq;

using Retrosharp.Contract.Franchise;
using Retrosharp.Contract.Standing;

namespace Retrosharp.Format.Standings
{
    /// <summary>
    /// Derives one all-time career summary per franchise lineage (every era sharing a
    /// <see cref="Franchise.FranchiseIdentifier"/> collapses into a single row, represented by
    /// its most recent era) from the full set of franchise eras and precomputed season
    /// standings. Pure logic, no I/O -- sibling to <see cref="StandingsResolver"/>. See
    /// spec/frontend-prototype.md's "Resolved: Franchise All-Time Summary" note.
    /// </summary>
    public static class FranchiseCareerSummaryResolver
    {
        public static IReadOnlyList<FranchiseCareerSummary> Resolve(
            IReadOnlyList<Franchise> franchiseEras,
            IReadOnlyList<FranchiseSeasonStanding> standings)
        {
            var standingsByFranchiseId = standings
                .GroupBy(s => s.FranchiseId)
                .ToDictionary(g => g.Key, g => (IReadOnlyList<FranchiseSeasonStanding>)g.ToList());

            var summaries = new List<FranchiseCareerSummary>();

            foreach (var lineage in franchiseEras.GroupBy(f => f.FranchiseIdentifier))
            {
                // Oldest first: FormerNames reads chronologically, and the last element is the
                // representative (most recent) era. A tie on FranchiseStart (shouldn't happen --
                // it's part of the era's own unique natural key -- but is possible if two
                // distinct identifiers were ever merged) breaks on Id for determinism.
                var eras = lineage.OrderBy(f => f.FranchiseStart).ThenBy(f => f.Id).ToList();
                var current = eras[^1];

                var lineageStandings = eras
                    .SelectMany(era => standingsByFranchiseId.GetValueOrDefault(era.Id, []))
                    .ToList();

                var wins = (short)lineageStandings.Sum(s => s.Wins);
                var losses = (short)lineageStandings.Sum(s => s.Losses);
                var ties = (short)lineageStandings.Sum(s => s.Ties);

                summaries.Add(new FranchiseCareerSummary
                {
                    FranchiseId = current.Id,
                    FranchiseCode = current.FranchiseCode,
                    CurrentName = $"{current.PlayingCity} {current.Nickname}",
                    FormerNames = eras
                        .Take(eras.Count - 1)
                        .Select(era => $"{era.PlayingCity} {era.Nickname}")
                        .Distinct()
                        .ToList(),
                    FirstSeasonYear = (short)eras[0].FranchiseStart.Year,
                    GamesPlayed = (short)(wins + losses + ties),
                    Wins = wins,
                    Losses = losses,
                    Ties = ties,
                    SeasonsAboveFiveHundred = lineageStandings.Count(s => s.WinPercentage > 0.5f),
                    SeasonsBelowFiveHundred = lineageStandings.Count(s => s.WinPercentage < 0.5f)
                });
            }

            return summaries;
        }
    }
}
