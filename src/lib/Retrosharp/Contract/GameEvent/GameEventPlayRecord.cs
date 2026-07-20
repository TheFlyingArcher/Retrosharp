using System.Collections.Generic;

namespace Retrosharp.Contract.GameEvent
{
    /// <summary>
    /// Bundles one resolved play and its runners so the whole graph can move from the
    /// resolver to the repository layer as a single unit. See
    /// <see cref="Retrosharp.Format.PlayByPlay.GameEventResolver"/> and spec/game-event.md,
    /// "Data Model".
    /// </summary>
    public sealed class GameEventPlayRecord
    {
        public required GameEvent Event { get; init; }

        public required IReadOnlyList<GameEventRunnerRecord> Runners { get; init; }
    }
}
