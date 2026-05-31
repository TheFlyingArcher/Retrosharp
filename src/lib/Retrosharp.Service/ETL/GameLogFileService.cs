using System;
using System.Collections.Generic;
using System.Text;

using CsvHelper;
using Retrosharp.Format;
using Retrosharp.Service.Interface.ETL;

namespace Retrosharp.Service.ETL
{
    public class GameLogFileService : IRetrosheetFileService<GameLog>
    {
        public async Task<IEnumerable<GameLog>> ParseFileAsync(string retrosheetFilePath)
        {
            if (!File.Exists(retrosheetFilePath))
                throw new FileNotFoundException($"The file at path '{retrosheetFilePath}' was not found.");

            try
            {
                using var reader = new StreamReader(retrosheetFilePath);
                using var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture);

                csv.Context.RegisterClassMap<GameLogMap>();
                var record = await csv.GetRecordsAsync<GameLog>().ToListAsync();
                return record;
            }
            catch (CsvHelperException ex)
            {
                throw new FormatException($"An error occurred while parsing the file at path '{retrosheetFilePath}'. See inner exception for details.", ex);
            }
        }
    }
}
