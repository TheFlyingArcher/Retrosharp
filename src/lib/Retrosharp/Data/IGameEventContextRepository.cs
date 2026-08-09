using Retrosharp.Contract.GameEvent;

namespace Retrosharp.Data
{
    /// <summary>
    /// Read access to <see cref="GameEventContext"/> (currently just per-game start time).
    /// Writes happen directly within <see cref="IGameEventRepository.BulkInsertAsync"/>,
    /// alongside GameSubstitution/GameAdjustment/GameComment -- this repository exists purely
    /// so consumers like game summary don't need direct DbContext access. See spec/api.md,
    /// "Game start time is parsed and discarded, not stored".
    /// </summary>
    public interface IGameEventContextRepository
    {
        /// <summary>
        /// Gets the given game's context metadata, or null if no event file has been imported
        /// for this game (distinct from an imported file having no parseable start time, which
        /// is a present row with a null <see cref="GameEventContext.StartTimeLocal"/>).
        /// </summary>
        Task<GameEventContext?> GetByGameIdAsync(int gameId);
    }
}
