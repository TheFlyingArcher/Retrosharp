using System;
using System.Collections.Generic;
using System.Text;

using Retrosharp.Contract.Game;

namespace Retrosharp.Data
{
    /// <summary>
    /// Represents a repository for Game entities, exposing query operations to retrieve games by home or visiting
    /// franchise.
    /// </summary>
    /// <remarks>Extends IRepository<Game>. Implementations should encapsulate data access and provide
    /// asynchronous query methods focused on franchise-based retrieval.</remarks>
    public interface IGameRepository : IRepository<Game>
    {
        /// <summary>
        /// Retrieves games for the specified visiting franchise.
        /// </summary>
        /// <param name="visitorFranchiseId">The identifier of the visiting franchise.</param>
        /// <returns>A Task that represents the asynchronous operation. The task result contains an IEnumerable<Game> of games
        /// where the specified franchise is the visitor.</returns>
        Task<IEnumerable<Game>> GetByVisitorFranchiseIdAsync(int visitorFranchiseId);
        
        /// <summary>
        /// Retrieves games for the specified home franchise.
        /// </summary>
        /// <param name="homeFranchiseId">The identifier of the home franchise.</param>
        /// <returns>A Task that represents the asynchronous operation. The task result contains an IEnumerable<Game> of games
        /// where the specified franchise is the home.</returns>
        Task<IEnumerable<Game>> GetByHomeFranchiseIdAsync(int homeFranchiseId);
    }
}
