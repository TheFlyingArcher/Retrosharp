using System;

namespace Retrosharp.Service.Interface
{
    /// <summary>
    /// Parses Retrosheet's biofile and populates Person, per spec/person.md. Distinct from
    /// IPersonService, which covers search/lookup rather than bulk ETL import.
    /// </summary>
    public interface IPersonImportService
    {
        /// <summary>
        /// Parses the biofile at the given path and inserts or updates Person records, matched
        /// by Retrosheet ID, as a single atomic operation. Safe to call repeatedly.
        /// </summary>
        Task<PersonImportResult> ImportAsync(string filePath);
    }
}
