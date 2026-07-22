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

        /// <summary>
        /// Retrieves the raw per-play fields needed to derive a pitcher's per-franchise-season
        /// event aggregate (see <see cref="Format.PlayByPlay.PitcherEventAggregateResolver"/>),
        /// optionally scoped to one season. See spec/api.md, "PitcherEventAggregate".
        /// </summary>
        /// <param name="personId">The pitcher's person ID.</param>
        /// <param name="season">The season year to scope to, or null for the pitcher's whole career.</param>
        Task<IEnumerable<PitcherGameEventRecord>> GetPitcherGameEventsAsync(int personId, short? season);

        /// <summary>
        /// Counts home runs allowed across every pitcher for the given franchises in one season --
        /// the league-wide input to <see cref="Format.PlayByPlay.FipConstantCalculator"/>.
        /// </summary>
        /// <param name="franchiseIds">The franchises belonging to the league.</param>
        /// <param name="season">The season year.</param>
        Task<int> GetLeagueHomerunsAllowedAsync(IEnumerable<int> franchiseIds, short season);

        /// <summary>
        /// Distinct game IDs where the given person batted at least once, optionally scoped to
        /// one season. See spec/api.md, "Player game logs are derived on demand".
        /// </summary>
        Task<IEnumerable<int>> GetGameIdsAsBatterAsync(int personId, short? season);

        /// <summary>
        /// Distinct game IDs where the given person pitched at least once, optionally scoped to
        /// one season.
        /// </summary>
        Task<IEnumerable<int>> GetGameIdsAsPitcherAsync(int personId, short? season);

        /// <summary>
        /// Reconstructs the complete play-by-play (every play, every runner) for every requested
        /// game in a single batched query, keyed by GameId -- <see cref="Format.PlayByPlay.GameStatisticsResolver"/>
        /// needs a whole game's plays regardless of which single player is being queried, so this
        /// can't be filtered down to just one player's rows. Fielding credits are deliberately not
        /// fetched, since nothing this supports (the player game log) uses them.
        /// </summary>
        Task<IReadOnlyDictionary<int, GamePlayByPlay>> GetGamesPlayByPlayAsync(IEnumerable<int> gameIds);
    }
}
