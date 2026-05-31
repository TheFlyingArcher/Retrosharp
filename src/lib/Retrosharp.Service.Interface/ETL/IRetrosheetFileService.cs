using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Retrosharp.Format;

namespace Retrosharp.Service.Interface.ETL
{
    /// <summary>
    /// Defines the contract for Retrosheet file parsing services.
    /// </summary>
    /// <typeparam name="F">The type of Retrosheet file being parsed.</typeparam>
    public interface IRetrosheetFileService<F>
        where F : RetrosheetFile
    {
        /// <summary>
        /// Asynchronously parses a Retrosheet file and returns the parsed records.
        /// </summary>
        /// <param name="retrosheetFilePath">The path to the Retrosheet file to parse.</param>
        /// <returns>A collection of parsed records of type F.</returns>
        Task<IEnumerable<F>> ParseFileAsync(string retrosheetFilePath);
    }
}
