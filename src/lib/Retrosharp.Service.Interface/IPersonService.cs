using Retrosharp.Contract.Person;

namespace Retrosharp.Service.Interface
{
    /// <summary>
    /// Service interface for managing person-related business logic.
    /// </summary>
    public interface IPersonService : IEntityService<Person>
    {
        /// <summary>
        /// Searches for people by name (surname, use name, or full name).
        /// </summary>
        /// <param name="searchTerm">The name or partial name to search for.</param>
        /// <returns>A collection of people matching the search criteria.</returns>
        Task<IEnumerable<Person>> SearchByNameAsync(string searchTerm);

        /// <summary>
        /// Gets a person by their Retrosheet ID.
        /// </summary>
        /// <param name="retrosheetId">The Retrosheet ID of the person.</param>
        /// <returns>The person with the specified Retrosheet ID, or null if not found.</returns>
        Task<Person> GetByRetrosheetIdAsync(string retrosheetId);

        /// <summary>
        /// Gets complete career statistics for a player including batting, pitching, and fielding.
        /// </summary>
        /// <param name="personId">The ID of the person.</param>
        /// <returns>The person with their complete statistics loaded.</returns>
        Task<Person> GetWithCareerStatsAsync(int personId);
    }
}
