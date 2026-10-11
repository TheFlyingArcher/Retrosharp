using System.Collections.Generic;
using System.Linq;

using Retrosharp.Contract.Franchise;
using Retrosharp.Contract.Standing;

namespace Retrosharp.Format.Standings
{
    /// <summary>
    /// Derives every former name of every franchise lineage from the full set of franchise eras
    /// and precomputed season standings. A name is a distinct (FranchiseLocation, Nickname) pair
    /// within one <see cref="Franchise.FranchiseIdentifier"/>, linked to the name that directly
    /// followed its last era; eras differing only by division,
    /// league or code share a name, and a name recurring after a gap is one name. Pure logic, no
    /// I/O -- sibling to <see cref="FranchiseCareerSummaryResolver"/>. See
    /// spec/former-franchise-names.md.
    /// </summary>
    public static class FormerFranchiseNameResolver
    {
        public static IReadOnlyList<FormerFranchiseName> Resolve(
            IReadOnlyList<Franchise> franchiseEras,
            IReadOnlyList<FranchiseSeasonStanding> standings)
        {
            var standingsByFranchiseId = standings
                .GroupBy(s => s.FranchiseId)
                .ToDictionary(g => g.Key, g => (IReadOnlyList<FranchiseSeasonStanding>)g.ToList());

            var formerNames = new List<FormerFranchiseName>();

            foreach (var lineage in franchiseEras.GroupBy(f => f.FranchiseIdentifier))
            {
                var eras = lineage.OrderBy(f => f.FranchiseStart).ThenBy(f => f.Id).ToList();
                var current = eras[^1];
                var currentName = NameOf(current);

                var formerGroups = eras
                    .Where(era => NameOf(era) != currentName)
                    .GroupBy(NameOf);

                foreach (var group in formerGroups)
                {
                    var nameEras = group.ToList();
                    var last = nameEras[^1];
                    var next = eras.Skip(eras.IndexOf(last) + 1).First(era => NameOf(era) != group.Key);

                    var nameStandings = nameEras
                        .SelectMany(era => standingsByFranchiseId.GetValueOrDefault(era.Id, []))
                        .ToList();

                    var wins = (short)nameStandings.Sum(s => s.Wins);
                    var losses = (short)nameStandings.Sum(s => s.Losses);
                    var ties = (short)nameStandings.Sum(s => s.Ties);

                    formerNames.Add(new FormerFranchiseName
                    {
                        FranchiseId = last.Id,
                        FranchiseCode = last.FranchiseCode,
                        Name = group.Key,
                        FirstSeasonYear = (short)nameEras[0].FranchiseStart.Year,
                        LastSeasonYear = (short)nameEras.Max(era => era.FranchiseEnd ?? era.FranchiseStart).Year,
                        NextFranchiseId = next.Id,
                        NextName = NameOf(next),
                        GamesPlayed = (short)(wins + losses + ties),
                        Wins = wins,
                        Losses = losses,
                        Ties = ties
                    });
                }
            }

            return formerNames;
        }

        private static string NameOf(Franchise era)
        {
            return $"{era.FranchiseLocation} {era.Nickname}";
        }
    }
}
