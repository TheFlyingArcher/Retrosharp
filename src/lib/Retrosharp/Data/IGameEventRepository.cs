using System.Collections.Generic;

using Retrosharp.Contract.GameEvent;

namespace Retrosharp.Data
{
    /// <summary>
    /// Repository for persisting resolved play-by-play data (GameEvent, GameEventRunner,
    /// GameEventFieldingCredit), its non-play context records (GameSubstitution,
    /// GameAdjustment, GameComment), and applying the derived Batting/Pitching/Fielding
    /// statistics behind the GameEventGameStatus atomic claim. See spec/game-event.md,
    /// "Data Model", and spec/phase-1-build-plan.md Steps 6b/6c/6d.
    /// </summary>
    public interface IGameEventRepository
    {
        /// <summary>
        /// Inserts a batch of resolved games -- each with its full play/runner/fielding-credit
        /// graph plus substitutions/adjustments/comments, one atomic transaction per game -- and
        /// then attempts to claim and apply each game's derived statistics (see
        /// <see cref="IGameStatisticsRepository"/>). A game already having any GameEvent rows is
        /// treated as fully processed and skipped for the event-data insert, rather than
        /// upserted; the statistics claim is attempted for every game regardless, since it's
        /// independently idempotent (see spec/phase-1-build-plan.md Step 6d).
        /// </summary>
        /// <param name="records">The resolved game event records to insert.</param>
        /// <returns>The number of games inserted/skipped for event data, and the number of games whose statistics were newly applied/already claimed.</returns>
        Task<(int GamesInserted, int GamesSkipped, int StatisticsApplied, int StatisticsSkipped)> BulkInsertAsync(IEnumerable<GameEventRecord> records);
    }
}
