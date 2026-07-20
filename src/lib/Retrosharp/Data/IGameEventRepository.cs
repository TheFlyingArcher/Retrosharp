using System.Collections.Generic;

using Retrosharp.Contract.GameEvent;

namespace Retrosharp.Data
{
    /// <summary>
    /// Repository for persisting resolved play-by-play data (GameEvent, GameEventRunner,
    /// GameEventFieldingCredit) and its non-play context records (GameSubstitution,
    /// GameAdjustment, GameComment). See spec/game-event.md, "Data Model", and
    /// spec/phase-1-build-plan.md Steps 6b/6c.
    /// </summary>
    public interface IGameEventRepository
    {
        /// <summary>
        /// Inserts a batch of resolved games -- each with its full play/runner/fielding-credit
        /// graph plus substitutions/adjustments/comments -- as a single atomic transaction. A
        /// game already having any GameEvent rows is treated as fully processed and skipped
        /// whole, rather than upserted -- this is the game-level "already processed" check
        /// appropriate to Step 6b/6c's scope; the stronger atomic claim needed to guard against
        /// two concurrent sagas racing on a shared game (GameEventGameStatus) is Step 6d's job.
        /// </summary>
        /// <param name="records">The resolved game event records to insert.</param>
        /// <returns>The number of games inserted and the number skipped as already present.</returns>
        Task<(int GamesInserted, int GamesSkipped)> BulkInsertAsync(IEnumerable<GameEventRecord> records);
    }
}
