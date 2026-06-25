using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CsvHelper;
using Retrosharp.Format;
using Retrosharp.Service.Interface.ETL;

namespace Retrosharp.Service.ETL
{
    public class BioFileService : IRetrosheetFileService<BioFile>
    {
        public async Task<IEnumerable<BioFile>> ParseFileAsync(string retrosheetFilePath)
        {
            if (!File.Exists(retrosheetFilePath))
                throw new FileNotFoundException($"The file at path '{retrosheetFilePath}' was not found.");

            try
            {
                using var reader = new StreamReader(retrosheetFilePath);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                csv.Context.RegisterClassMap<BioFileMapping>();
                var records = await csv.GetRecordsAsync<BioFile>().ToListAsync();
                return records;
            }
            catch (CsvHelperException ex)
            {
                throw new FormatException($"An error occurred while parsing the bio file at path '{retrosheetFilePath}'. See inner exception for details.", ex);
            }
        }
    }
}
