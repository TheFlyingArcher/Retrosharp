using System.Collections.Generic;

namespace Retrosharp.Contract.GameEvent
{
    /// <summary>
    /// Bundles one resolved runner and its fielding credits so the pair can move from the
    /// resolver to the repository layer as a single unit. See
    /// <see cref="Retrosharp.Format.PlayByPlay.GameEventResolver"/> and spec/game-event.md,
    /// "Data Model".
    /// </summary>
    public sealed class GameEventRunnerRecord
    {
        public required GameEventRunner Runner { get; init; }

        public required IReadOnlyList<GameEventFieldingCredit> FieldingCredits { get; init; }
    }
}
