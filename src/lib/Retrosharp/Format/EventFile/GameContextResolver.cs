using System;
using System.Collections.Generic;

using Retrosharp.Contract.GameEvent;

namespace Retrosharp.Format.EventFile
{
    /// <summary>
    /// Resolves an <see cref="EventFileGame"/>'s non-play context records -- substitutions,
    /// the five adjustment record types, and free-text commentary -- into the Contract classes
    /// built for them in Step 1. Unlike <see cref="PlayByPlay.GameEventResolver"/>, this needs
    /// no baserunner/lineup state tracking: each record maps almost directly to its Contract
    /// shape. Pure logic, no I/O -- the database-lookup boundary (resolving Retrosheet IDs to
    /// PersonIds) is the caller's job, same convention as <see cref="PlayByPlay.GameEventResolver"/>.
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

            foreach (var record in game.Records)
            {
                switch (record)
                {
                    // StartRecord is deliberately excluded -- it's the starting lineup, not a
                    // substitution ("a player entering the game *mid-game*").
                    case SubRecord sub:
                        substitutions.Add(new GameSubstitution
                        {
                            GameId = gameId,
                            Sequence = ++substitutionSequence,
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
                            AdjustmentType = MapAdjustmentType(adjustment.AdjustmentTypeCode),
                            PersonId = ResolvePersonId(adjustment.RetrosheetId, personIdsByRetrosheetId),
                            Value = adjustment.Value
                        });
                        break;

                    case ComRecord comment:
                        comments.Add(new GameComment
                        {
                            GameId = gameId,
                            Sequence = ++commentSequence,
                            CommentText = comment.CommentText
                        });
                        break;
                }
            }

            return (substitutions, adjustments, comments);
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
