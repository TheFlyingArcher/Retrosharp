using System;
using System.Collections.Generic;
using System.Text;

using Retrosharp.Contract;

namespace Retrosharp.Service.Interface
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEntityService<T>
        where T: Entity
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Get a single entity by the ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The entity matching the ID or null if not found</returns>
        public Task<T> GetByIdAsync(int id);

        /// <summary>
        /// Save an entity to the data store
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public Task<T> SaveAsync(T entity);
    }
}
