using System;
using System.Collections.Generic;
using System.Text;

using Retrosharp.Format;

namespace Retrosharp.Service.Interface.ETL
{
    public interface IRetrosheetFileService<T>
        where T: RetrosheetFile
    {
        /// <summary>
        /// Parse a Retrosheet data file
        /// </summary>
        /// <param name="retrosheetFilePath">Path to the Retrosheet file</param>
        /// <returns>The completed parsed file</returns>
        Task<IEnumerable<T>> ParseFileAsync(string retrosheetFilePath);
    }
}
