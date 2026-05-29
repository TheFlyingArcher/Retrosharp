using System;
using System.Collections.Generic;
using System.Text;

using Retrosharp.Format;
using Retrosharp.Service.Interface.ETL;

namespace Retrosharp.Service.ETL
{
    internal abstract class RetrosheetFileService<F> : IRetrosheetFileService<F>
        where F : RetrosheetFile
    {
        public abstract Task<IEnumerable<F>> ParseFileAsync(string retrosheetFilePath);
    }
}
