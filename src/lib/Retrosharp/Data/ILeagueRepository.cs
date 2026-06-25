using Retrosharp.Contract.League;

namespace Retrosharp.Data
{
    /// <summary>
    /// Repository interface for managing League entities in the data store.
    /// </summary>
    public interface ILeagueRepository : IRepository<League>
    {
        /// <summary>
        /// Retrieves a league by its league code.
        /// </summary>
        /// <param name="leagueCode">The league code to search for.</param>
        /// <returns>The league with the specified code, or null if not found.</returns>
        Task<League> GetByLeagueCodeAsync(string leagueCode);
    }
}
