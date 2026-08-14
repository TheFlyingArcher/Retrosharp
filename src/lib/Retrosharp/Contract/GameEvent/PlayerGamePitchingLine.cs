using System;

using Retrosharp.Contract.Pitching;

namespace Retrosharp.Contract.GameEvent
{
    /// <summary>
    /// One game's pitching line for a single player, derived on demand from that game's
    /// play-by-play rather than stored. <see cref="Stats"/>.<c>EarnedRuns</c> is the
    /// independently-computed figure (<see cref="Format.PlayByPlay.GameReconciliationResolver.ResolveIndependentEarnedRuns"/>),
    /// not the authoritative per-game "data,er,..." record value -- that raw figure was only
    /// ever used transiently during import to build the season aggregate and was never
    /// persisted at the per-game grain. See spec/api.md.
    /// </summary>
    public sealed class PlayerGamePitchingLine
    {
        public required int GameId { get; init; }

        public required DateTime GameDate { get; init; }

        public required bool IsHome { get; init; }

        public string FranchiseCode { get; set; } = string.Empty;

        public string OpponentFranchiseCode { get; set; } = string.Empty;

        /// <summary>
        /// Defensive position(s) played in this specific game, comma-separated if more than one
        /// -- raw Retrosheet position codes (resolved the same way as
        /// <c>GameSummaryService.ResolvePosition</c>), not display abbreviations. Almost always
        /// "1" (pitcher) for a Pitching line, but not assumed -- a pitcher can be moved to
        /// another position mid-game without leaving the game.
        /// </summary>
        public string? Position { get; set; }

        public required PitchingDelta Stats { get; init; }
    }
}
