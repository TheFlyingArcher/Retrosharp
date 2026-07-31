using System.Collections.Generic;

using Retrosharp.Contract.GameEvent;

namespace Retrosharp.Data
{
    /// <summary>
    /// A repository which provides data access methods for GameAdjustment entities.
    /// </summary>
    public interface IGameAdjustmentRepository : IRepository<GameAdjustment>
    {
        /// <summary>
        /// Gets every adjustment for a given game, ordered by RecordIndex (the game's full
        /// Retrosheet record list position -- see <see cref="GameEvent.RecordIndex"/> -- not
        /// this table's own, type-scoped Sequence). See spec/api.md, "GET /games/{gameId}/events".
        /// </summary>
        Task<IEnumerable<GameAdjustment>> GetByGameIdAsync(int gameId);
    }
}
