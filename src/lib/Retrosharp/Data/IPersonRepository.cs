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
        /// Searches for people by surname (case-insensitive partial match).
        /// </summary>
        /// <param name="surname">The surname or partial surname to search for.</param>
        /// <returns>A collection of people matching the search criteria.</returns>
        Task<IEnumerable<Person>> SearchBySurnameAsync(string surname);

        /// <summary>
        /// Searches for people by any name field (surname, use name, full name) with case-insensitive partial matching.
        /// </summary>
        /// <param name="searchTerm">The search term to match against name fields.</param>
        /// <returns>A collection of people matching the search criteria.</returns>
        Task<IEnumerable<Person>> SearchByNameAsync(string searchTerm);

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
