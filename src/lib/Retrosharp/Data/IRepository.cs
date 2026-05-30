using System;
using System.Collections.Generic;
using System.Text;

using Retrosharp.Contract;

namespace Retrosharp.Data
{
    /// <summary>
    /// Represents a repository that provides asynchronous create, read, update, and delete operations for entities of
    /// type TC.
    /// </summary>
    /// <remarks>Implementations should perform I/O asynchronously and preserve transactional and concurrency
    /// semantics appropriate to the underlying data store.</remarks>
    /// <typeparam name="TC">The entity type stored and managed by the repository. Must derive from Entity.</typeparam>
    public interface IRepository<TC>
        where TC: Entity
    {
        /// <summary>
        /// Insert a new entity into the repository and return the created entity with its assigned ID.
        /// </summary>
        /// <param name="entity">The new entity to insert into the database</param>
        /// <returns>The created entity with its assigned ID</returns>
        Task<TC> CreateAsync(TC entity);

        /// <summary>
        /// Retrieve all entities from the repository.
        /// </summary>
        /// <returns>An enumerable object containing all entities</returns>
        Task<IEnumerable<TC>> GetAllAsync();

        /// <summary>
        /// Retrieve an entity from the repository by its ID.
        /// </summary>
        /// <param name="id">The ID of the entity to retrieve</param>
        /// <returns>The entity with the specified ID</returns>
        Task<TC> GetByIdAsync(int id);

        /// <summary>
        /// Update an existing entity in the repository.
        /// </summary>
        /// <param name="entity">The entity to update</param>
        /// <returns>The updated entity</returns>
        Task<TC> UpdateAsync(TC entity);

        /// <summary>
        /// Delete an entity from the repository by its ID.
        /// </summary>
        /// <param name="id">The ID of the entity to delete</param>
        Task DeleteAsync(int id);
    }
}
