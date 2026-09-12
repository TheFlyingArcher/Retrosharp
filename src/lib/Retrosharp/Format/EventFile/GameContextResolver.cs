using System;
using System.Collections.Generic;

using Retrosharp.Contract.GameEvent;

namespace Retrosharp.Format.EventFile
{
    /// <summary>
    /// Resolves an <see cref="EventFileGame"/>'s non-play context records -- substitutions,
    /// the five adjustment record types, and free-text commentary -- into the Contract classes
    /// built for them in Step 1. Each record maps almost directly to its Contract shape; the
    /// one piece of running state needed is a batting-order-slot map, because an "ladj"
    /// (batting out of order) record names a lineup slot rather than a player and the proper
    /// batter has to be recovered from whoever currently occupies that slot. Pure logic, no
    /// I/O -- the database-lookup boundary (resolving Retrosheet IDs to PersonIds) is the
    /// caller's job, same convention as <see cref="PlayByPlay.GameEventResolver"/>.
    /// See spec/game-event.md, "Data Model", and spec/phase-1-build-plan.md Step 6c.
    /// </summary>
    public static class GameContextResolver
    {
        public static (IReadOnlyList<GameSubstitution> Substitutions, IReadOnlyList<GameAdjustment> Adjustments, IReadOnlyList<GameComment> Comments) Resolve(
            int gameId,
            EventFileGame game,
            IReadOnlyDictionary<string, int> personIdsByRetrosheetId)
        {
            var substitutions = new List<GameSubstitution>();
            var adjustments = new List<GameAdjustment>();
            var comments = new List<GameComment>();

            var substitutionSequence = 0;
            var adjustmentSequence = 0;
            var commentSequence = 0;

            // Who currently occupies each batting-order slot, per team -- the minimal lineup
            // state an "ladj" record needs (see ResolveAdjustmentPersonId). Seeded by "start"
            // records and kept current by "sub" records.
            var visitorBattingSlots = new Dictionary<byte, string>();
            var homeBattingSlots = new Dictionary<byte, string>();

            for (var i = 0; i < game.Records.Count; i++)
            {
                switch (game.Records[i])
                {
                    // StartRecord is deliberately excluded from the substitutions list -- it's
                    // the starting lineup, not a player entering *mid-game* -- but it still
                    // seeds the batting-order-slot map.
                    case StartRecord start:
                        TrackBattingSlot(start, visitorBattingSlots, homeBattingSlots);
                        break;

                    case SubRecord sub:
                        TrackBattingSlot(sub, visitorBattingSlots, homeBattingSlots);
                        substitutions.Add(new GameSubstitution
                        {
                            GameId = gameId,
                            Sequence = ++substitutionSequence,
                            RecordIndex = i,
                            PersonId = ResolvePersonId(sub.RetrosheetId, personIdsByRetrosheetId),
                            TeamAtBat = sub.IsHomeTeam ? "H" : "V",
                            BattingOrderPosition = sub.BattingOrder,
                            FieldingPosition = sub.Position
                        });
                        break;

                    case AdjustmentRecord adjustment:
                        adjustments.Add(new GameAdjustment
                        {
                            GameId = gameId,
                            Sequence = ++adjustmentSequence,
                            RecordIndex = i,
                            AdjustmentType = MapAdjustmentType(adjustment.AdjustmentTypeCode),
                            PersonId = ResolveAdjustmentPersonId(adjustment, visitorBattingSlots, homeBattingSlots, personIdsByRetrosheetId),
                            Value = adjustment.Value
                        });
                        break;

                    case ComRecord comment:
                        comments.Add(new GameComment
                        {
                            GameId = gameId,
                            Sequence = ++commentSequence,
                            RecordIndex = i,
                            CommentText = comment.CommentText
                        });
                        break;
                }
            }

            return (substitutions, adjustments, comments);
        }

        /// <summary>
        /// Records the player now occupying a batting-order slot. <see cref="LineupRecord.BattingOrder"/>
        /// 0 means the player has no slot (for example a reliever in a DH game) and is ignored;
        /// every "start"/"sub" with a real slot -- including a pinch runner (position 12), who
        /// genuinely inherits that slot -- overwrites the previous occupant.
        /// </summary>
        private static void TrackBattingSlot(
            LineupRecord record,
            IDictionary<byte, string> visitorBattingSlots,
            IDictionary<byte, string> homeBattingSlots)
        {
            if (record.BattingOrder == 0)
                return;

            (record.IsHomeTeam ? homeBattingSlots : visitorBattingSlots)[record.BattingOrder] = record.RetrosheetId;
        }

        /// <summary>
        /// Every adjustment record except "ladj" carries a Retrosheet player id in its first
        /// field. "ladj" (batting out of order) instead carries the batting team there
        /// (0 = visitor, 1 = home) and, in <see cref="AdjustmentRecord.Value"/>, the
        /// batting-order slot (1-9) of the batter who was due up -- so it resolves to whoever
        /// currently occupies that slot, an id that (being a starter or substitute) is already
        /// present in <paramref name="personIdsByRetrosheetId"/>.
        /// </summary>
        private static int ResolveAdjustmentPersonId(
            AdjustmentRecord adjustment,
            IReadOnlyDictionary<byte, string> visitorBattingSlots,
            IReadOnlyDictionary<byte, string> homeBattingSlots,
            IReadOnlyDictionary<string, int> personIdsByRetrosheetId)
        {
            if (adjustment.AdjustmentTypeCode != "ladj")
                return ResolvePersonId(adjustment.RetrosheetId, personIdsByRetrosheetId);

            var battingSlots = adjustment.RetrosheetId switch
            {
                "0" => visitorBattingSlots,
                "1" => homeBattingSlots,
                _ => throw new InvalidOperationException(
                    $"'ladj' record has batting team '{adjustment.RetrosheetId}', expected '0' (visitor) or '1' (home).")
            };

            if (!byte.TryParse(adjustment.Value, out var slot) || slot is < 1 or > 9)
                throw new InvalidOperationException(
                    $"'ladj' record has batting-order slot '{adjustment.Value}', expected a number 1-9.");

            if (!battingSlots.TryGetValue(slot, out var retrosheetId))
                throw new InvalidOperationException(
                    $"'ladj' record references batting-order slot {slot}, but no 'start'/'sub' record has filled that slot yet.");

            return ResolvePersonId(retrosheetId, personIdsByRetrosheetId);
        }

        private static GameAdjustmentType MapAdjustmentType(string adjustmentTypeCode) => adjustmentTypeCode switch
        {
            "badj" => GameAdjustmentType.BattingHandedness,
            "padj" => GameAdjustmentType.PitchingHandedness,
            "ladj" => GameAdjustmentType.LineupPosition,
            "radj" => GameAdjustmentType.RunnerPlacement,
            "presadj" => GameAdjustmentType.PitcherResponsibility,
            _ => throw new InvalidOperationException($"Unrecognized adjustment record type '{adjustmentTypeCode}'.")
        };

        private static int ResolvePersonId(string retrosheetId, IReadOnlyDictionary<string, int> personIdsByRetrosheetId)
        {
            if (!personIdsByRetrosheetId.TryGetValue(retrosheetId, out var personId))
                throw new InvalidOperationException($"No Person found for Retrosheet ID '{retrosheetId}'.");

            return personId;
        }
    }
}
