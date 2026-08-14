using Retrosharp.Contract.Person;
using System;
using System.Collections.Generic;
using System.Text;

namespace Retrosharp.Data
{
    /// <summary>
    /// Repository interface for managing Person entities in the data store.
    /// </summary>
    public interface IPersonRepository : IRepository<Person>
    {
        /// <summary>
        /// Retrieves a person by their Retrosheet ID.
        /// </summary>
        /// <param name="retrosheetId">The Retrosheet ID of the person to retrieve.</param>
        /// <returns>The person with the specified Retrosheet ID, or null if not found.</returns>
        Task<Person> GetByRetrosheetIdAsync(string retrosheetId);

        /// <summary>
        /// Browses people ordered by surname, optionally restricted to surnames starting with a
        /// given letter (case-insensitive). Used by the Players page's A-Z browse list, as
        /// opposed to <see cref="SearchByNameAsync"/>'s free-text search.
        /// </summary>
        /// <param name="letter">If provided, only surnames starting with this letter are returned.</param>
        /// <param name="limit">Maximum number of results to return.</param>
        /// <param name="offset">Number of matching results to skip.</param>
        /// <returns>The page of people matching the criteria, and the total number of matches.</returns>
        Task<(IEnumerable<Person> Items, int TotalCount)> BrowseBySurnameAsync(char? letter, int limit, int offset);

        /// <summary>
        /// Searches for people by any name field (surname, use name, full name) with case-insensitive partial matching.
        /// </summary>
        /// <param name="searchTerm">The search term to match against name fields.</param>
        /// <param name="limit">Maximum number of results to return.</param>
        /// <param name="offset">Number of matching results to skip.</param>
        /// <returns>The page of people matching the search criteria, and the total number of matches.</returns>
        Task<(IEnumerable<Person> Items, int TotalCount)> SearchByNameAsync(string searchTerm, int limit, int offset);

        /// <summary>
        /// Inserts or updates the given people, matched by Retrosheet ID, as a single atomic
        /// transaction covering the entire batch. Used by the Person parser, where an
        /// unrecoverable error must leave no partial data committed.
        /// </summary>
        /// <param name="people">The people to insert or update.</param>
        /// <returns>The number of people added and the number updated.</returns>
        Task<(int Added, int Updated)> BulkUpsertAsync(IEnumerable<Person> people);
    }
}
