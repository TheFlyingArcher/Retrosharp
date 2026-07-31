using System.Collections.Generic;

using Retrosharp.Contract.GameEvent;

namespace Retrosharp.Data
{
    /// <summary>
    /// A repository which provides data access methods for GameSubstitution entities.
    /// </summary>
    public interface IGameSubstitutionRepository : IRepository<GameSubstitution>
    {
        /// <summary>
        /// Gets every substitution for a given game, ordered by RecordIndex (the game's full
        /// Retrosheet record list position -- see <see cref="GameEvent.RecordIndex"/> -- not
        /// this table's own, type-scoped Sequence). See spec/api.md, "GET /games/{gameId}/events".
        /// </summary>
        Task<IEnumerable<GameSubstitution>> GetByGameIdAsync(int gameId);
    }
}
