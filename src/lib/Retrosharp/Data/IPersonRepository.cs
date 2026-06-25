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
        Task<Person> GetByRetrosheetId(string retrosheetId);
    }
}
