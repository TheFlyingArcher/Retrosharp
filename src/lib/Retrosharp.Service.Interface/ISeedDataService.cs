using System;

namespace Retrosharp.Service.Interface
{
    /// <summary>
    /// Populates Franchise and Ballpark from Retrosheet's static reference CSV files.
    /// League is seeded separately via an EF Core migration (see RetrosharpContext).
    /// See spec/seed-data.md.
    /// </summary>
    public interface ISeedDataService
    {
        /// <summary>
        /// Imports franchises and ballparks from their reference CSV files, skipping any
        /// record that already exists by natural key. Safe to call repeatedly.
        /// </summary>
        Task<SeedDataResult> SeedAsync();
    }
}
