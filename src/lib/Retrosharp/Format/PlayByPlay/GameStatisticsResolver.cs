using System.Collections.Generic;
using System.Linq;

using Retrosharp.Contract.Batting;
using Retrosharp.Contract.Fielding;
using Retrosharp.Contract.GameEvent;
using Retrosharp.Contract.Pitching;

namespace Retrosharp.Format.PlayByPlay
{
    /// <summary>
    /// Derives one game's Batting/Pitching/Fielding statistical contribution from its already-
    /// resolved play-by-play, as deltas to be added on top of whatever a player's season row
    /// already holds. Pure logic, no I/O -- sibling to <see cref="GameEventResolver"/>/
    /// <see cref="EventFile.GameContextResolver"/>. See spec/game-event.md and
    /// spec/phase-1-build-plan.md Step 6d for the exact derivation rules and the explicit
    /// scope exclusions (Pitching.Saves, Fielding.PassedBalls/DoublePlays/TriplePlays,
    /// Batting.Positions -- all left at their default value here, not computed).
    /// </summary>
    public static class GameStatisticsResolver
    {
        /// <summary>
        /// Events that end a plate appearance. Shared with <see cref="PitcherEventAggregateResolver"/>,
        /// which applies the same rule from the pitcher's side (batters faced against, rather than
        /// plate appearances for) -- see spec/api.md, "PitcherEventAggregate".
        /// </summary>
        internal static readonly HashSet<GameEventType> PlateAppearanceEndingEvents =
        [
            GameEventType.Single, GameEventType.Double, GameEventType.Triple, GameEventType.HomeRun,
            GameEventType.Walk, GameEventType.IntentionalWalk, GameEventType.HitByPitch,
            GameEventType.Strikeout, GameEventType.GroundOut, GameEventType.FlyOut,
            GameEventType.Error, GameEventType.FieldersChoice, GameEventType.CatcherInterference
        ];

        /// <summary>
        /// Plate-appearance-ending events that don't count as an official at-bat. Shared with
        /// <see cref="PitcherEventAggregateResolver"/> -- see spec/api.md, "PitcherEventAggregate".
        /// </summary>
        internal static readonly HashSet<GameEventType> NonAtBatEvents =
        [
            GameEventType.Walk, GameEventType.IntentionalWalk, GameEventType.HitByPitch, GameEventType.CatcherInterference
        ];

        public static GameStatisticsDelta Resolve(
            int homeFranchiseId,
            int visitorFranchiseId,
            short seasonYear,
            IReadOnlyList<GameEventPlayRecord> plays,
            IReadOnlyDictionary<int, short> earnedRunsByPitcherId)
        {
            var batting = new Dictionary<int, BattingAccumulator>();
            var pitching = new Dictionary<int, PitchingAccumulator>();
            var fielding = new Dictionary<(int PersonId, byte Position), FieldingAccumulator>();

            // The starting pitcher for a team is whoever is PitcherId on the first play where
            // the OTHER team is batting; the "finishing" pitcher is whoever is PitcherId on the
            // last such play. Both are derivable purely from the play list -- no need for the
            // caller to separately supply the game's "start" records.
            var homeStartingPitcherId = plays.FirstOrDefault(p => p.Event.TeamAtBat == "V")?.Event.PitcherId;
            var visitorStartingPitcherId = plays.FirstOrDefault(p => p.Event.TeamAtBat == "H")?.Event.PitcherId;
            var homeFinishingPitcherId = plays.LastOrDefault(p => p.Event.TeamAtBat == "V")?.Event.PitcherId;
            var visitorFinishingPitcherId = plays.LastOrDefault(p => p.Event.TeamAtBat == "H")?.Event.PitcherId;

            foreach (var play in plays)
            {
                var evt = play.Event;
                var battingFranchiseId = evt.TeamAtBat == "H" ? homeFranchiseId : visitorFranchiseId;
                var fieldingFranchiseId = evt.TeamAtBat == "H" ? visitorFranchiseId : homeFranchiseId;

                var batterAcc = GetOrAdd(batting, evt.BatterId, battingFranchiseId);
                ApplyBatterEvent(batterAcc, evt);

                var pitcherAcc = GetOrAdd(pitching, evt.PitcherId, fieldingFranchiseId);
                ApplyPitcherEvent(pitcherAcc, evt.EventType);
                if (evt.SecondaryEventType is { } secondaryEventType)
                    ApplyPitcherEvent(pitcherAcc, secondaryEventType);

                var battersOwnRunner = play.Runners.FirstOrDefault(r => r.Runner.StartBase == BaseState.BattersBox);
                var outsOnThisPlay = play.Runners.Count(r => r.Runner.IsOut);

                foreach (var runnerRecord in play.Runners)
                {
                    var runner = runnerRecord.Runner;

                    if (runner.IsOut)
                        pitcherAcc.OutsRecorded++;

                    if (runner.EndBase == BaseState.Home && !runner.IsOut)
                    {
                        GetOrAdd(batting, runner.PersonId, battingFranchiseId).Runs++;

                        if (runner.ResponsiblePitcherId is { } responsiblePitcherId)
                            GetOrAdd(pitching, responsiblePitcherId, fieldingFranchiseId).Runs++;
                    }

                    // Checking SecondaryEventType too (not just EventType) is what makes a
                    // bundled "K+SB2"/"K+CS2(24)" count at all -- EventType alone can only ever
                    // be Strikeout/Walk for those plays. The StartBase != BattersBox guard keeps
                    // this correct now that a bundled play's Runners can include the batter
                    // (from the primary K/W) alongside the actual base-stealer (from the
                    // secondary SB/CS) -- a real steal is never the batter's own row.
                    var isStolenBaseEvent = evt.EventType == GameEventType.StolenBase || evt.SecondaryEventType == GameEventType.StolenBase;
                    var isCaughtStealingEvent = evt.EventType is GameEventType.CaughtStealing or GameEventType.PickoffCaughtStealing
                        || evt.SecondaryEventType is GameEventType.CaughtStealing or GameEventType.PickoffCaughtStealing;

                    if (isStolenBaseEvent && runner.StartBase != BaseState.BattersBox)
                    {
                        GetOrAdd(batting, runner.PersonId, battingFranchiseId).StolenBases++;
                    }
                    else if (isCaughtStealingEvent && runner.IsOut && runner.StartBase != BaseState.BattersBox)
                    {
                        GetOrAdd(batting, runner.PersonId, battingFranchiseId).TimesCaughtStealing++;
                    }

                    foreach (var credit in runnerRecord.FieldingCredits)
                    {
                        var fieldingAcc = GetOrAdd(fielding, (credit.PersonId, credit.Position), fieldingFranchiseId);
                        switch (credit.CreditType)
                        {
                            case FieldingCreditType.Putout: fieldingAcc.Putouts++; break;
                            case FieldingCreditType.Assist: fieldingAcc.Assists++; break;
                            case FieldingCreditType.Error: fieldingAcc.Errors++; break;
                        }
                    }
                }

                // Grounded into double play: the batter's own at-bat ended in a GroundOut, and
                // at least one *other* runner was also put out on the same play.
                if (evt.EventType == GameEventType.GroundOut && battersOwnRunner is { Runner.IsOut: true } && outsOnThisPlay >= 2)
                    batterAcc.GroundedIntoDoublePlay++;
            }

            foreach (var (personId, pitcherAcc) in pitching)
            {
                var isStarter = personId == homeStartingPitcherId || personId == visitorStartingPitcherId;
                var isFinisher = personId == homeFinishingPitcherId || personId == visitorFinishingPitcherId;

                pitcherAcc.Started = isStarter;
                // "Games Finished" credits a reliever who closes out the game, not a starter
                // who throws a complete game -- that's tracked separately by CompleteGames.
                pitcherAcc.Finished = isFinisher && !isStarter;
                pitcherAcc.CompleteGame = isStarter && isFinisher;
                pitcherAcc.EarnedRuns = earnedRunsByPitcherId.GetValueOrDefault(personId);
            }

            var battingDeltas = batting.Select(kv => new BattingDelta
            {
                PersonId = kv.Key,
                FranchiseId = kv.Value.FranchiseId,
                SeasonYear = seasonYear,
                PlateAppearances = kv.Value.PlateAppearances,
                AtBats = kv.Value.AtBats,
                Hits = kv.Value.Hits,
                Doubles = kv.Value.Doubles,
                Triples = kv.Value.Triples,
                Homeruns = kv.Value.Homeruns,
                BaseOnBalls = kv.Value.BaseOnBalls,
                Strikeouts = kv.Value.Strikeouts,
                SacrificeFlies = kv.Value.SacrificeFlies,
                SacrificeBunts = kv.Value.SacrificeBunts,
                IntentionalBb = kv.Value.IntentionalBb,
                HitByPitches = kv.Value.HitByPitches,
                StolenBases = kv.Value.StolenBases,
                TimesCaughtStealing = kv.Value.TimesCaughtStealing,
                Runs = kv.Value.Runs,
                GroundedIntoDoublePlay = kv.Value.GroundedIntoDoublePlay
            }).ToList();

            var pitchingDeltas = pitching.Select(kv => new PitchingDelta
            {
                PersonId = kv.Key,
                FranchiseId = kv.Value.FranchiseId,
                SeasonYear = seasonYear,
                GamesPitched = 1,
                GamesStarted = (short)(kv.Value.Started ? 1 : 0),
                GamesFinished = (short)(kv.Value.Finished ? 1 : 0),
                CompleteGames = (short)(kv.Value.CompleteGame ? 1 : 0),
                Shutouts = (short)(kv.Value.CompleteGame && kv.Value.Runs == 0 ? 1 : 0),
                InningsPitched = kv.Value.OutsRecorded,
                Hits = kv.Value.Hits,
                Runs = kv.Value.Runs,
                EarnedRuns = kv.Value.EarnedRuns,
                BaseOnBalls = kv.Value.BaseOnBalls,
                Strikeouts = kv.Value.Strikeouts,
                IntentionalBb = kv.Value.IntentionalBb,
                HitBatsmen = kv.Value.HitBatsmen,
                Balks = kv.Value.Balks,
                WildPitches = kv.Value.WildPitches
            }).ToList();

            var fieldingDeltas = fielding.Select(kv => new FieldingDelta
            {
                PersonId = kv.Key.PersonId,
                FranchiseId = kv.Value.FranchiseId,
                SeasonYear = seasonYear,
                Position = kv.Key.Position,
                Putouts = kv.Value.Putouts,
                Assists = kv.Value.Assists,
                Errors = kv.Value.Errors
            }).ToList();

            return new GameStatisticsDelta { Battings = battingDeltas, Pitchings = pitchingDeltas, Fieldings = fieldingDeltas };
        }

        private static void ApplyBatterEvent(BattingAccumulator acc, GameEvent evt)
        {
            if (!PlateAppearanceEndingEvents.Contains(evt.EventType))
                return;

            acc.PlateAppearances++;

            if (!NonAtBatEvents.Contains(evt.EventType) && !evt.IsSacHit && !evt.IsSacFly)
                acc.AtBats++;

            switch (evt.EventType)
            {
                case GameEventType.Single: acc.Hits++; break;
                case GameEventType.Double: acc.Hits++; acc.Doubles++; break;
                case GameEventType.Triple: acc.Hits++; acc.Triples++; break;
                case GameEventType.HomeRun: acc.Hits++; acc.Homeruns++; break;
                case GameEventType.Walk: acc.BaseOnBalls++; break;
                case GameEventType.IntentionalWalk: acc.BaseOnBalls++; acc.IntentionalBb++; break;
                case GameEventType.HitByPitch: acc.HitByPitches++; break;
                case GameEventType.Strikeout: acc.Strikeouts++; break;
            }

            if (evt.IsSacFly)
                acc.SacrificeFlies++;
            if (evt.IsSacHit)
                acc.SacrificeBunts++;
        }

        // Takes a bare GameEventType (not the whole GameEvent) so the same logic can be applied
        // once for evt.EventType and, for a bundled play, a second time for
        // evt.SecondaryEventType (e.g. "K+WP" -- the wild pitch would otherwise never be
        // counted, since the play's own EventType can only ever be Strikeout).
        private static void ApplyPitcherEvent(PitchingAccumulator acc, GameEventType eventType)
        {
            switch (eventType)
            {
                case GameEventType.Single or GameEventType.Double or GameEventType.Triple or GameEventType.HomeRun:
                    acc.Hits++;
                    break;
                case GameEventType.Walk:
                    acc.BaseOnBalls++;
                    break;
                case GameEventType.IntentionalWalk:
                    acc.BaseOnBalls++;
                    acc.IntentionalBb++;
                    break;
                case GameEventType.Strikeout:
                    acc.Strikeouts++;
                    break;
                case GameEventType.HitByPitch:
                    acc.HitBatsmen++;
                    break;
                case GameEventType.Balk:
                    acc.Balks++;
                    break;
                case GameEventType.WildPitch:
                    acc.WildPitches++;
                    break;
            }
        }

        private static T GetOrAdd<TKey, T>(Dictionary<TKey, T> accumulators, TKey key, int franchiseId)
            where TKey : notnull
            where T : IAccumulator, new()
        {
            if (!accumulators.TryGetValue(key, out var acc))
            {
                acc = new T { FranchiseId = franchiseId };
                accumulators[key] = acc;
            }

            return acc;
        }

        private interface IAccumulator
        {
            int FranchiseId { get; set; }
        }

        private sealed class BattingAccumulator : IAccumulator
        {
            public int FranchiseId { get; set; }
            public short PlateAppearances;
            public short AtBats;
            public short Hits;
            public short Doubles;
            public short Triples;
            public short Homeruns;
            public short BaseOnBalls;
            public short Strikeouts;
            public short SacrificeFlies;
            public short SacrificeBunts;
            public short IntentionalBb;
            public short HitByPitches;
            public short StolenBases;
            public short TimesCaughtStealing;
            public short Runs;
            public short GroundedIntoDoublePlay;
        }

        private sealed class PitchingAccumulator : IAccumulator
        {
            public int FranchiseId { get; set; }
            public bool Started;
            public bool Finished;
            public bool CompleteGame;
            public short OutsRecorded;
            public short Hits;
            public short Runs;
            public short EarnedRuns;
            public short BaseOnBalls;
            public short Strikeouts;
            public short IntentionalBb;
            public short HitBatsmen;
            public short Balks;
            public short WildPitches;
        }

        private sealed class FieldingAccumulator : IAccumulator
        {
            public int FranchiseId { get; set; }
            public int Putouts;
            public int Assists;
            public int Errors;
        }
    }
}
