using System;
using System.Collections.Generic;
using System.Text;

namespace Retrosharp.Configuration
{
    public sealed class RetrosharpConfiguration
    {
        public RetrosharpConfiguration()
        {
            ConnectionString = string.Empty;
        }

        public string ConnectionString { get; set; }
    }
}
