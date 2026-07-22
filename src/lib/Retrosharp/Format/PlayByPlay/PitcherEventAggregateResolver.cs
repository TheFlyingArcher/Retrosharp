using System.Collections.Generic;
using System.Linq;

using Retrosharp.Contract.GameEvent;

namespace Retrosharp.Format.PlayByPlay
{
    /// <summary>
    /// Derives a pitcher's per-franchise-season event aggregate (home runs/fly balls/at-bats/
    /// sacrifice flies allowed) from already-fetched <see cref="PitcherGameEventRecord"/> rows.
    /// Pure logic, no I/O -- sibling to <see cref="GameStatisticsResolver"/>. See spec/api.md,
    /// "PitcherEventAggregate".
    /// </summary>
    public static class PitcherEventAggregateResolver
    {
        public static IReadOnlyList<PitcherEventAggregate> Resolve(int personId, IEnumerable<PitcherGameEventRecord> events)
        {
            return events
                .GroupBy(e => (e.FranchiseId, e.SeasonYear))
                .Select(g => new PitcherEventAggregate
                {
                    PersonId = personId,
                    FranchiseId = g.Key.FranchiseId,
                    SeasonYear = g.Key.SeasonYear,
                    HomerunsAllowed = g.Count(e => e.EventType == GameEventType.HomeRun),
                    FlyBallsAllowed = g.Count(e => e.BattedBallType == BattedBallType.FlyBall),
                    AtBatsAgainst = g.Count(IsAtBat),
                    SacrificeFliesAgainst = g.Count(e => e.IsSacFly)
                })
                .ToList();
        }

        // Mirrors GameStatisticsResolver.ApplyBatterEvent's at-bat rule exactly, applied from
        // the pitcher's side instead of the batter's.
        private static bool IsAtBat(PitcherGameEventRecord e) =>
            GameStatisticsResolver.PlateAppearanceEndingEvents.Contains(e.EventType)
            && !GameStatisticsResolver.NonAtBatEvents.Contains(e.EventType)
            && !e.IsSacHit
            && !e.IsSacFly;
    }
}
