using System.Collections.Generic;

namespace Retrosharp.Contract.Game
{
    /// <summary>
    /// Bundles one parsed game log row's full object graph -- the game itself plus its
    /// lineups and team-level statistics -- so it can move from the Game Log import service to
    /// the repository layer as a single unit. See spec/game-log.md.
    /// </summary>
    public sealed class GameLogRecord
    {
        public required Game Game { get; init; }

        public required IReadOnlyList<GameLineup> Lineups { get; init; }

        public required IReadOnlyList<GameBattingStatistics> BattingStatistics { get; init; }

        public required IReadOnlyList<GamePitchingStatistics> PitchingStatistics { get; init; }

        public required IReadOnlyList<GameFieldingStatistics> FieldingStatistics { get; init; }
    }
}
