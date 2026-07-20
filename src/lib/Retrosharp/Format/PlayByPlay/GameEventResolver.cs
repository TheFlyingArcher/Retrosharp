using System;
using System.Collections.Generic;
using System.Linq;

using Retrosharp.Contract.GameEvent;
using Retrosharp.Format.EventFile;

namespace Retrosharp.Format.PlayByPlay
{
    /// <summary>
    /// Resolves a <see cref="EventFileGame"/>'s records -- combined with Step 6a's
    /// <see cref="PlayCodeParser"/> -- into fully-identified <see cref="GameEventPlayRecord"/>s,
    /// filling in every <c>PersonId</c> that <see cref="ParsedPlay"/> deliberately leaves out.
    /// Pure logic: no I/O, no repository dependency -- the database-lookup boundary is the
    /// caller's job (resolve every distinct Retrosheet ID once, hand in the resulting
    /// dictionary). See spec/game-event.md, "Data Model", and spec/phase-1-build-plan.md
    /// Step 6b.
    /// </summary>
    public static class GameEventResolver
    {
        public static IReadOnlyList<GameEventPlayRecord> Resolve(
            int gameId,
            EventFileGame game,
            IReadOnlyDictionary<string, int> personIdsByRetrosheetId)
        {
            var visitingTeam = new TeamLineupState();
            var homeTeam = new TeamLineupState();

            // A "sub"/"radj" record between two plays of different half-innings belongs to the
            // *upcoming* half, not the one that just ended -- for example, a new pitcher enters
            // and the extra-innings tiebreaker runner is placed before the half-inning's first
            // play. Precomputing each record's effective half-inning by scanning backward (the
            // next PlayRecord at or after it) lets the forward pass reset the baserunner
            // tracker at the true boundary, before any such record mutates it.
            var halfInningKeys = new (byte Inning, bool IsHomeTeamBatting)?[game.Records.Count];
            (byte Inning, bool IsHomeTeamBatting)? next = null;
            for (var i = game.Records.Count - 1; i >= 0; i--)
            {
                if (game.Records[i] is PlayRecord boundary)
                    next = (boundary.Inning, boundary.IsHomeTeamBatting);

                halfInningKeys[i] = next;
            }

            var baserunners = new Dictionary<BaseState, RunnerOccupant>();
            (byte Inning, bool IsHomeTeamBatting)? currentHalfInning = null;

            var plays = new List<GameEventPlayRecord>();
            var sequence = 0;

            for (var i = 0; i < game.Records.Count; i++)
            {
                var key = halfInningKeys[i];
                if (key != currentHalfInning)
                {
                    baserunners.Clear();
                    currentHalfInning = key;
                }

                switch (game.Records[i])
                {
                    case StartRecord start:
                        ApplyLineupRecord(start, visitingTeam, homeTeam, baserunners);
                        break;

                    case SubRecord sub:
                        ApplyLineupRecord(sub, visitingTeam, homeTeam, baserunners);
                        break;

                    case AdjustmentRecord { AdjustmentTypeCode: "radj" } radj:
                        ApplyRunnerPlacement(radj, currentHalfInning, visitingTeam, homeTeam, personIdsByRetrosheetId, baserunners);
                        break;

                    case AdjustmentRecord:
                        // badj/padj/ladj/presadj: recognized, not acted on. See
                        // AdjustmentRecord's doc comment.
                        break;

                    case PlayRecord play:
                        sequence++;
                        plays.Add(ResolvePlay(gameId, sequence, play, visitingTeam, homeTeam, personIdsByRetrosheetId, baserunners));
                        break;

                    // ComRecord/DataRecord: not part of GameEvent resolution (Step 6c/6e).
                }
            }

            return plays;
        }

        private static GameEventPlayRecord ResolvePlay(
            int gameId,
            int sequence,
            PlayRecord play,
            TeamLineupState visitingTeam,
            TeamLineupState homeTeam,
            IReadOnlyDictionary<string, int> personIdsByRetrosheetId,
            Dictionary<BaseState, RunnerOccupant> baserunners)
        {
            var battingTeam = play.IsHomeTeamBatting ? homeTeam : visitingTeam;
            var fieldingTeam = play.IsHomeTeamBatting ? visitingTeam : homeTeam;

            var currentPitcherId = ResolvePersonId(fieldingTeam.RequirePositionPlayer(1), personIdsByRetrosheetId);
            var batterId = ResolvePersonId(play.RetrosheetId, personIdsByRetrosheetId);

            var parsed = PlayCodeParser.Parse(play.RawEventText, play.CountField, play.PitchSequence);

            var gameEvent = new GameEvent
            {
                GameId = gameId,
                Sequence = sequence,
                Inning = play.Inning,
                TeamAtBat = play.IsHomeTeamBatting ? "H" : "V",
                BatterId = batterId,
                PitcherId = currentPitcherId,
                Balls = parsed.Balls,
                Strikes = parsed.Strikes,
                FoulBallsWithTwoStrikes = parsed.FoulBallsWithTwoStrikes,
                PitchSequence = play.PitchSequence,
                RawEventText = parsed.RawEventText,
                EventType = parsed.EventType,
                BattedBallType = parsed.BattedBallType,
                IsSacHit = parsed.IsSacHit,
                IsSacFly = parsed.IsSacFly
            };

            // Two passes against a snapshot of pre-play state: a single play can move multiple
            // runners at once (for example, second-to-third while third-to-home on the same
            // play), so every runner's identity/responsible pitcher must be resolved from the
            // *pre-play* occupancy before any base is overwritten.
            var resolvedRunners = new List<(ParsedRunnerAdvance Advance, string RetrosheetId, byte BattingSlot, int ResponsiblePitcherId)>();
            foreach (var advance in parsed.Runners)
            {
                if (advance.StartBase == BaseState.BattersBox)
                {
                    var battingSlot = battingTeam.RequireSlotForPlayer(play.RetrosheetId);
                    resolvedRunners.Add((advance, play.RetrosheetId, battingSlot, currentPitcherId));
                }
                else
                {
                    if (!baserunners.TryGetValue(advance.StartBase, out var occupant))
                        throw new InvalidOperationException(
                            $"Play '{play.RawEventText}' (inning {play.Inning}) references a runner on {advance.StartBase} " +
                            "that the resolver has no record of -- a preceding play or substitution was missed. " +
                            $"Current baserunners: [{string.Join(", ", baserunners.Select(kv => $"{kv.Key}={kv.Value.RetrosheetId}(slot{kv.Value.BattingSlot})"))}]");

                    resolvedRunners.Add((advance, occupant.RetrosheetId, occupant.BattingSlot, occupant.ResponsiblePitcherId));
                }
            }

            var runnerRecords = new List<GameEventRunnerRecord>();
            foreach (var (advance, retrosheetId, battingSlot, responsiblePitcherId) in resolvedRunners)
            {
                var personId = ResolvePersonId(retrosheetId, personIdsByRetrosheetId);

                var runner = new GameEventRunner
                {
                    PersonId = personId,
                    StartBase = advance.StartBase,
                    EndBase = advance.EndBase,
                    IsOut = advance.IsOut,
                    IsRBI = advance.IsRBI,
                    IsEarnedRun = advance.IsEarnedRun,
                    // "Null unless the runner scores" -- see GameEventRunner.ResponsiblePitcherId.
                    ResponsiblePitcherId = advance.EndBase == BaseState.Home ? responsiblePitcherId : null
                };

                List<GameEventFieldingCredit> fieldingCredits;
                try
                {
                    fieldingCredits = advance.FieldingCredits
                        .Select(credit => new GameEventFieldingCredit
                        {
                            PersonId = ResolvePersonId(fieldingTeam.RequirePositionPlayer(credit.Position), personIdsByRetrosheetId),
                            CreditType = credit.CreditType,
                            Sequence = credit.Sequence,
                            Position = credit.Position
                        })
                        .ToList();
                }
                catch (InvalidOperationException ex)
                {
                    throw new InvalidOperationException(
                        $"Play '{play.RawEventText}' (inning {play.Inning}): {ex.Message}", ex);
                }

                runnerRecords.Add(new GameEventRunnerRecord { Runner = runner, FieldingCredits = fieldingCredits });
            }

            // Applying occupancy changes is itself two full passes, not one pass per advance:
            // a play's advances aren't guaranteed to be listed in a base-safe order (a walk's
            // own batter-to-first advance can be listed *before* an existing runner's
            // first-to-second advance in the same play), so removing a start base right after
            // adding a different runner to that same base -- interleaved per-advance -- can
            // evict the wrong occupant. Removing every vacated base first, then adding every
            // new occupant, makes the result independent of list order.
            foreach (var (advance, _, _, _) in resolvedRunners)
            {
                if (advance.StartBase != BaseState.BattersBox)
                    baserunners.Remove(advance.StartBase);
            }

            foreach (var (advance, retrosheetId, battingSlot, responsiblePitcherId) in resolvedRunners)
            {
                if (!advance.IsOut && advance.EndBase is BaseState.First or BaseState.Second or BaseState.Third)
                    baserunners[advance.EndBase] = new RunnerOccupant(retrosheetId, battingSlot, responsiblePitcherId);
            }

            return new GameEventPlayRecord { Event = gameEvent, Runners = runnerRecords };
        }

        private static void ApplyLineupRecord(
            LineupRecord record,
            TeamLineupState visitingTeam,
            TeamLineupState homeTeam,
            Dictionary<BaseState, RunnerOccupant> baserunners)
        {
            var team = record.IsHomeTeam ? homeTeam : visitingTeam;

            switch (record.Position)
            {
                case >= 1 and <= 9:
                    team.PositionToRetrosheetId[record.Position] = record.RetrosheetId;
                    if (record.BattingOrder != 0)
                        team.SlotToRetrosheetId[record.BattingOrder] = record.RetrosheetId;
                    break;

                case 10: // designated hitter -- no field position
                case 11: // pinch hitter -- no field position (yet)
                    if (record.BattingOrder != 0)
                        team.SlotToRetrosheetId[record.BattingOrder] = record.RetrosheetId;
                    break;

                case 12: // pinch runner -- updates the batting slot, and swaps the physical
                         // occupant of whichever base that slot's runner is currently on,
                         // preserving the original ResponsiblePitcherId (the runner's
                         // "inherited runner" charge doesn't change just because a different
                         // person is now the one standing on the base).
                    if (record.BattingOrder != 0)
                    {
                        team.SlotToRetrosheetId[record.BattingOrder] = record.RetrosheetId;

                        var baseToSwap = baserunners
                            .Where(kv => kv.Value.BattingSlot == record.BattingOrder)
                            .Select(kv => (BaseState?)kv.Key)
                            .FirstOrDefault();

                        if (baseToSwap != null)
                        {
                            var existing = baserunners[baseToSwap.Value];
                            baserunners[baseToSwap.Value] = existing with { RetrosheetId = record.RetrosheetId };
                        }
                    }
                    break;
            }
        }

        private static void ApplyRunnerPlacement(
            AdjustmentRecord radj,
            (byte Inning, bool IsHomeTeamBatting)? currentHalfInning,
            TeamLineupState visitingTeam,
            TeamLineupState homeTeam,
            IReadOnlyDictionary<string, int> personIdsByRetrosheetId,
            Dictionary<BaseState, RunnerOccupant> baserunners)
        {
            if (currentHalfInning == null)
                throw new InvalidOperationException("A 'radj' record was encountered with no following play to establish which half-inning it belongs to.");

            var battingTeam = currentHalfInning.Value.IsHomeTeamBatting ? homeTeam : visitingTeam;
            var fieldingTeam = currentHalfInning.Value.IsHomeTeamBatting ? visitingTeam : homeTeam;

            var battingSlot = battingTeam.RequireSlotForPlayer(radj.RetrosheetId);
            var responsiblePitcherId = ResolvePersonId(fieldingTeam.RequirePositionPlayer(1), personIdsByRetrosheetId);
            var placedBase = (BaseState)byte.Parse(radj.Value);

            baserunners[placedBase] = new RunnerOccupant(radj.RetrosheetId, battingSlot, responsiblePitcherId);
        }

        private static int ResolvePersonId(string retrosheetId, IReadOnlyDictionary<string, int> personIdsByRetrosheetId)
        {
            if (!personIdsByRetrosheetId.TryGetValue(retrosheetId, out var personId))
                throw new InvalidOperationException($"No Person found for Retrosheet ID '{retrosheetId}'.");

            return personId;
        }

        private sealed record RunnerOccupant(string RetrosheetId, byte BattingSlot, int ResponsiblePitcherId);

        private sealed class TeamLineupState
        {
            public Dictionary<byte, string> SlotToRetrosheetId { get; } = new();

            public Dictionary<byte, string> PositionToRetrosheetId { get; } = new();

            public string RequirePositionPlayer(byte position)
            {
                if (!PositionToRetrosheetId.TryGetValue(position, out var retrosheetId))
                    throw new InvalidOperationException($"No player is currently recorded at field position {position}.");

                return retrosheetId;
            }

            public byte RequireSlotForPlayer(string retrosheetId)
            {
                foreach (var (slot, id) in SlotToRetrosheetId)
                {
                    if (id == retrosheetId)
                        return slot;
                }

                throw new InvalidOperationException($"Player '{retrosheetId}' is not currently recorded in any batting-order slot.");
            }
        }
    }
}
