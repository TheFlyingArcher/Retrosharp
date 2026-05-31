using System;
using System.Collections.Generic;
using System.Text;

using Retrosharp.Format;
using Retrosharp.Service.Interface.ETL;

namespace Retrosharp.Service.ETL
{
    public class BioFileService : IRetrosheetFileService<BioFile>
    {
        public async Task<IEnumerable<BioFile>> ParseFileAsync(string retrosheetFilePath)
        {
            throw new NotImplementedException();
        }
    }
}
