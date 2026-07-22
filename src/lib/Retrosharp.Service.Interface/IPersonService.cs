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
        /// <param name="limit">Maximum number of results to return.</param>
        /// <param name="offset">Number of matching results to skip.</param>
        /// <returns>The page of people matching the search criteria, and the total number of matches.</returns>
        Task<(IEnumerable<Person> Items, int TotalCount)> SearchByNameAsync(string searchTerm, int limit, int offset);

        /// <summary>
        /// Gets a person by their Retrosheet ID.
        /// </summary>
        /// <param name="retrosheetId">The Retrosheet ID of the person.</param>
        /// <returns>The person with the specified Retrosheet ID, or null if not found.</returns>
        Task<Person> GetByRetrosheetIdAsync(string retrosheetId);
    }
}
