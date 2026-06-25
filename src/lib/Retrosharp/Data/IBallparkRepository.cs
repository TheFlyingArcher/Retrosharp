using Retrosharp.Contract.Ballpark;

namespace Retrosharp.Data
{
    /// <summary>
    /// Repository interface for managing Ballpark entities in the data store.
    /// </summary>
    public interface IBallparkRepository : IRepository<Ballpark>
    {
        /// <summary>
        /// Retrieves a ballpark by its site code.
        /// </summary>
        /// <param name="siteCode">The site code to search for.</param>
        /// <returns>The ballpark with the specified site code, or null if not found.</returns>
        Task<Ballpark> GetBySiteCodeAsync(string siteCode);
    }
}
