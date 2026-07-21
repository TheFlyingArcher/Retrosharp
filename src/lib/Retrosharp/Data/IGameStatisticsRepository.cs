using Retrosharp.Contract.GameEvent;

namespace Retrosharp.Data
{
    /// <summary>
    /// Applies one game's derived Batting/Pitching/Fielding statistics, guarded by the atomic
    /// GameEventGameStatus claim. See spec/game-event.md and spec/phase-1-build-plan.md
    /// Step 6d.
    /// </summary>
    public interface IGameStatisticsRepository
    {
        /// <summary>
        /// Attempts to atomically claim <paramref name="gameId"/> by inserting a row into
        /// GameEventGameStatus, and -- only if that claim succeeds -- applies
        /// <paramref name="delta"/> to Batting, Pitching, and Fielding, all within a single
        /// transaction. If the claim fails because another process already claimed this game
        /// (a unique-key violation on GameId, not a bug), no statistics are applied and this
        /// returns false; that game's statistics are someone else's job. Any other failure
        /// rolls back and propagates as a genuine error.
        /// </summary>
        /// <param name="gameId">The game whose statistics are being applied.</param>
        /// <param name="delta">The game's derived statistical contribution.</param>
        /// <returns>True if this call newly claimed and applied the game's statistics; false if it was already claimed.</returns>
        Task<bool> TryApplyGameStatisticsAsync(int gameId, GameStatisticsDelta delta);
    }
}
