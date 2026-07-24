using Retrosharp.Contract.Franchise;
using Retrosharp.Contract.Person;

namespace Retrosharp.Service.Interface
{
    /// <summary>
    /// Service interface for team (franchise) search, identity detail, and roster lookups.
    /// See spec/api.md, "GET /teams/search", "GET /teams/{id}", "GET /teams/{id}/roster".
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
    }
}
