using System.Collections.Generic;
using System.Linq;

using Retrosharp.Contract.GameEvent;

namespace Retrosharp.Format.PlayByPlay
{
    /// <summary>
    /// Derives, from an already-resolved play-by-play, the two figures Step 6e reconciles
    /// against data sourced independently by other parsers: team-level Batting/Pitching/
    /// Fielding totals (compared against the Game Log Parser's Game*Statistics) and per-pitcher
    /// earned runs computed straight from GameEventRunner.IsEarnedRun (compared against the
    /// event file's own "data,er,..." records, already sourced into Pitching.EarnedRuns by
    /// Step 6d). Pure logic, no I/O -- sibling to <see cref="GameStatisticsResolver"/>. See
    /// spec/game-event.md Requirements 177-178 and spec/phase-1-build-plan.md Step 6e.
    /// </summary>
    public static class GameReconciliationResolver
    {
        public static GameTeamStatisticsDelta ResolveTeamTotals(
            int homeFranchiseId,
            int visitorFranchiseId,
            IReadOnlyList<GameEventPlayRecord> plays,
            GameStatisticsDelta statistics)
        {
            var runsBattedInByFranchiseId = new Dictionary<int, short>
            {
                [homeFranchiseId] = 0,
                [visitorFranchiseId] = 0
            };

            foreach (var play in plays)
            {
                var battingFranchiseId = play.Event.TeamAtBat == "H" ? homeFranchiseId : visitorFranchiseId;
                var rbiCount = play.Runners.Count(r => r.Runner.IsRBI);
                runsBattedInByFranchiseId[battingFranchiseId] += (short)rbiCount;
            }

            var battingTotals = statistics.Battings
                .GroupBy(b => b.FranchiseId)
                .Select(g => new TeamBattingTotal
                {
                    FranchiseId = g.Key,
                    PlateAppearances = (short)g.Sum(b => b.PlateAppearances),
                    AtBats = (short)g.Sum(b => b.AtBats),
                    Hits = (short)g.Sum(b => b.Hits),
                    Doubles = (short)g.Sum(b => b.Doubles),
                    Triples = (short)g.Sum(b => b.Triples),
                    Homeruns = (short)g.Sum(b => b.Homeruns),
                    RunsBattedIn = runsBattedInByFranchiseId.GetValueOrDefault(g.Key),
                    BaseOnBalls = (short)g.Sum(b => b.BaseOnBalls),
                    Strikeouts = (short)g.Sum(b => b.Strikeouts),
                    SacrificeFlies = (short)g.Sum(b => b.SacrificeFlies),
                    SacrificeBunts = (short)g.Sum(b => b.SacrificeBunts),
                    IntentionalBb = (short)g.Sum(b => b.IntentionalBb),
                    HitByPitches = (short)g.Sum(b => b.HitByPitches),
                    StolenBases = (short)g.Sum(b => b.StolenBases),
                    TimesCaughtStealing = (short)g.Sum(b => b.TimesCaughtStealing),
                    Runs = (short)g.Sum(b => b.Runs),
                    GroundedIntoDoublePlay = (short)g.Sum(b => b.GroundedIntoDoublePlay)
                })
                .ToList();

            var pitchingTotals = statistics.Pitchings
                .GroupBy(p => p.FranchiseId)
                .Select(g => new TeamPitchingTotal
                {
                    FranchiseId = g.Key,
                    PitchersUsed = (byte)g.Count(),
                    IndividualEarnedRuns = (short)g.Sum(p => p.EarnedRuns),
                    WildPitches = (byte)g.Sum(p => p.WildPitches),
                    Balks = (byte)g.Sum(p => p.Balks)
                })
                .ToList();

            var fieldingTotals = statistics.Fieldings
                .GroupBy(f => f.FranchiseId)
                .Select(g => new TeamFieldingTotal
                {
                    FranchiseId = g.Key,
                    Putouts = (short)g.Sum(f => f.Putouts),
                    Assists = (short)g.Sum(f => f.Assists),
                    Errors = (short)g.Sum(f => f.Errors)
                })
                .ToList();

            return new GameTeamStatisticsDelta
            {
                Battings = battingTotals,
                Pitchings = pitchingTotals,
                Fieldings = fieldingTotals
            };
        }

        // Independent of Pitching.EarnedRuns (sourced from "data,er,..." records by Step 6d) --
        // this walks the same IsEarnedRun/ResponsiblePitcherId annotations already used for
        // GameStatisticsResolver's inherited-runner-aware Runs attribution, filtered to earned
        // runs only, so the two figures are genuinely independently derived.
        public static IReadOnlyDictionary<int, short> ResolveIndependentEarnedRuns(IReadOnlyList<GameEventPlayRecord> plays)
        {
            var earnedRunsByPitcherId = new Dictionary<int, short>();

            foreach (var play in plays)
            {
                foreach (var runnerRecord in play.Runners)
                {
                    var runner = runnerRecord.Runner;

                    if (runner.EndBase != BaseState.Home || runner.IsOut || !runner.IsEarnedRun)
                        continue;

                    if (runner.ResponsiblePitcherId is not { } responsiblePitcherId)
                        continue;

                    earnedRunsByPitcherId[responsiblePitcherId] = (short)(earnedRunsByPitcherId.GetValueOrDefault(responsiblePitcherId) + 1);
                }
            }

            return earnedRunsByPitcherId;
        }
    }
}
