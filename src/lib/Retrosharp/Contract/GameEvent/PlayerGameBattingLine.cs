using System;

using Retrosharp.Contract.Batting;

namespace Retrosharp.Contract.GameEvent
{
    /// <summary>
    /// One game's batting line for a single player, derived on demand from that game's
    /// play-by-play rather than stored. See spec/api.md, "Player game logs are derived on
    /// demand, not stored".
    /// </summary>
    public sealed class PlayerGameBattingLine
    {
        public required int GameId { get; init; }

        public required DateTime GameDate { get; init; }

        public required bool IsHome { get; init; }

        /// <summary>The player's own franchise for this game -- can vary game to game after a mid-season trade.</summary>
        public string FranchiseCode { get; set; } = string.Empty;

        public string OpponentFranchiseCode { get; set; } = string.Empty;

        /// <summary>
        /// Defensive position(s) played in this specific game, comma-separated if more than one
        /// -- raw Retrosheet position codes (resolved the same way as
        /// <c>GameSummaryService.ResolvePosition</c>), not display abbreviations. Null if the
        /// player didn't appear in the starting lineup or any substitution record for this game
        /// (for example, a pinch runner tracked only via play-by-play).
        /// </summary>
        public string? Position { get; set; }

        public required BattingDelta Stats { get; init; }
    }
}
