using System.Collections.Generic;
using System.Linq;

using Retrosharp.Contract.Game;
using Retrosharp.Contract.Standing;

namespace Retrosharp.Format.Standings
{
    /// <summary>
    /// Derives every participating franchise's regular-season standing for one season from that
    /// season's <c>Game</c> results. Pure logic, no I/O -- sibling to
    /// <see cref="Retrosharp.Format.PlayByPlay.GameStatisticsResolver"/>. See
    /// spec/frontend-prototype.md's "Resolved: Standings Derivation" note and
    /// spec/api.md for the full design (grouping, ranking, and the scope boundary around
    /// "Pennant").
    /// </summary>
    public static class StandingsResolver
    {
        /// <param name="games">Every game played in this season, regardless of franchise.</param>
        /// <param name="franchiseContext">
        /// Each participating franchise's league and division for this season -- already the
        /// era-specific <c>Franchise</c> row a <c>Game</c> resolves to at import time, so no
        /// further era resolution is needed here. A franchise with a null <c>LeagueId</c> is
        /// excluded from the result entirely, since it can't be ranked against anything.
        /// </param>
        public static IReadOnlyList<FranchiseSeasonStanding> Resolve(
            short seasonYear,
            IReadOnlyList<Game> games,
            IReadOnlyDictionary<int, (int? LeagueId, string? DivisionCode)> franchiseContext)
        {
            var records = Tally(games);

            var entries = new Dictionary<int, FranchiseSeasonStanding>();
            foreach (var (franchiseId, record) in records)
            {
                if (!franchiseContext.TryGetValue(franchiseId, out var context) || context.LeagueId is not { } leagueId)
                    continue;

                entries[franchiseId] = new FranchiseSeasonStanding
                {
                    FranchiseId = franchiseId,
                    SeasonYear = seasonYear,
                    Wins = record.Wins,
                    Losses = record.Losses,
                    Ties = record.Ties
                };
            }

            // League-wide ranking determines LeagueBestRecord (the pre-1969 pennant) for every
            // franchise, and doubles as Rank/GamesBehind for a franchise-season with no division.
            foreach (var leagueGroup in entries.Keys.GroupBy(id => franchiseContext[id].LeagueId!.Value))
            {
                ApplyRanking(
                    leagueGroup.Select(id => entries[id]).ToList(),
                    (entry, isLeader) => entry.LeagueBestRecord = isLeader,
                    applyRankAndGamesBehind: true);
            }

            // Division ranking overrides Rank/GamesBehind (and sets DivisionChampion) for any
            // franchise-season that actually has a division -- pre-1969 franchise-seasons keep
            // the league-wide values already applied above.
            var divisionGroups = entries.Keys
                .Where(id => franchiseContext[id].DivisionCode != null)
                .GroupBy(id => (franchiseContext[id].LeagueId!.Value, franchiseContext[id].DivisionCode));

            foreach (var divisionGroup in divisionGroups)
            {
                ApplyRanking(
                    divisionGroup.Select(id => entries[id]).ToList(),
                    (entry, isLeader) => entry.DivisionChampion = isLeader,
                    applyRankAndGamesBehind: true);
            }

            return entries.Values.ToList();
        }

        private static void ApplyRanking(
            IReadOnlyList<FranchiseSeasonStanding> group,
            System.Action<FranchiseSeasonStanding, bool> markLeader,
            bool applyRankAndGamesBehind)
        {
            // Deterministic tie-break: win percentage, then raw wins, then FranchiseId. A true
            // win-percentage tie is rare and this project has no tiebreaker-game data to break
            // it more meaningfully -- documented in spec/frontend-prototype.md.
            var ordered = group
                .OrderByDescending(e => e.WinPercentage)
                .ThenByDescending(e => e.Wins)
                .ThenBy(e => e.FranchiseId)
                .ToList();

            var leader = ordered[0];

            for (var i = 0; i < ordered.Count; i++)
            {
                var entry = ordered[i];
                markLeader(entry, i == 0);

                if (!applyRankAndGamesBehind)
                    continue;

                entry.Rank = (byte)(i + 1);
                entry.GamesBehind = ((decimal)(leader.Wins - entry.Wins) + (entry.Losses - leader.Losses)) / 2m;
            }
        }

        private static Dictionary<int, (short Wins, short Losses, short Ties)> Tally(IReadOnlyList<Game> games)
        {
            var records = new Dictionary<int, (short Wins, short Losses, short Ties)>();

            foreach (var game in games)
            {
                if (game.HomeTeamRuns > game.VisitorRuns)
                {
                    Increment(records, game.HomeFranchiseId, wins: 1);
                    Increment(records, game.VisitorFranchiseId, losses: 1);
                }
                else if (game.VisitorRuns > game.HomeTeamRuns)
                {
                    Increment(records, game.VisitorFranchiseId, wins: 1);
                    Increment(records, game.HomeFranchiseId, losses: 1);
                }
                else
                {
                    Increment(records, game.HomeFranchiseId, ties: 1);
                    Increment(records, game.VisitorFranchiseId, ties: 1);
                }
            }

            return records;
        }

        private static void Increment(
            Dictionary<int, (short Wins, short Losses, short Ties)> records,
            int franchiseId, short wins = 0, short losses = 0, short ties = 0)
        {
            records.TryGetValue(franchiseId, out var record);
            records[franchiseId] = ((short)(record.Wins + wins), (short)(record.Losses + losses), (short)(record.Ties + ties));
        }
    }
}
