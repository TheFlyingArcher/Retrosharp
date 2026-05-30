using System;
using System.Collections.Generic;
using System.Text;

namespace Retrosharp.Message
{
    public abstract class BaseMessage
    {
        public Guid RequestId { get; set; }
    }
}
