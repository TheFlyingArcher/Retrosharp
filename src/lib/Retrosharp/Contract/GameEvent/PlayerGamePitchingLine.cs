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

        public required PitchingDelta Stats { get; init; }
    }
}
