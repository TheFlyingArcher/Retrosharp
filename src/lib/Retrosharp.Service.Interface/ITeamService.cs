using Retrosharp.Contract.Franchise;
using Retrosharp.Contract.Game;
using Retrosharp.Contract.Person;

namespace Retrosharp.Service.Interface
{
    /// <summary>
    /// Service interface for team (franchise) search, identity detail, roster, and manager
    /// history lookups. See spec/api.md, "GET /teams/search", "GET /teams/{id}",
    /// "GET /teams/{id}/roster", "GET /teams/{id}/managers".
    /// </summary>
    public interface ITeamService
    {
        Task<(IEnumerable<Franchise> Items, int TotalCount)> SearchAsync(string? q, string? code, short? season, int limit, int offset);

        Task<Franchise> GetByIdAsync(int id);

        /// <summary>
        /// Gets every player who recorded a Batting, Pitching, or Fielding row for the given
        /// franchise, for one season or across the franchise's whole history when
        /// <paramref name="season"/> is null.
        /// </summary>
        Task<IEnumerable<Person>> GetRosterAsync(int franchiseId, short? season);

        /// <summary>
        /// Gets the manager(s) who ran the given franchise during the given season, in
        /// chronological order. Consecutive games under the same manager collapse into a single
        /// entry; a mid-season managerial change produces more than one. See spec/api.md,
        /// "Team-season manager history is not yet exposed".
        /// </summary>
        Task<IEnumerable<ManagerTenure>> GetManagerHistoryAsync(int franchiseId, short season);
    }
}
