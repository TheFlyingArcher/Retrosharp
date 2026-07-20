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

        /// <summary>
        /// Inserts a batch of parsed game log records -- each with its full lineup and
        /// statistics graph -- as a single atomic transaction. Records whose natural key
        /// (GameDate, GameNumber, HomeFranchiseId, VisitorFranchiseId) already exists are
        /// skipped rather than updated, since a completed historical game's recorded stats
        /// don't change the way a person's biographical data can.
        /// </summary>
        /// <param name="records">The parsed game log records to insert.</param>
        /// <returns>The number of games added and the number skipped as already present.</returns>
        Task<(int Added, int Skipped)> BulkInsertAsync(IEnumerable<GameLogRecord> records);

        /// <summary>
        /// Resolves a game by its natural key -- the same key used for Game Log idempotency
        /// and required by the Game Event Parser to map a play-by-play file's "id" record
        /// (team code + date + game number) to an existing Game row.
        /// </summary>
        /// <param name="gameDate">The game's date.</param>
        /// <param name="gameNumber">The game number (0 for a single game, 1/2 for a doubleheader).</param>
        /// <param name="homeFranchiseId">The home franchise's Id.</param>
        /// <param name="visitorFranchiseId">The visiting franchise's Id.</param>
        /// <returns>The matching Game, or null if no such game exists yet.</returns>
        Task<Game> GetByNaturalKeyAsync(DateTime gameDate, byte gameNumber, int homeFranchiseId, int visitorFranchiseId);
    }
}
