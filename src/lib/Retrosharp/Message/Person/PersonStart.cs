using System;

namespace Retrosharp.Message.Person
{
    public class PersonStart : BaseMessage, IMessage
    {
        public PersonStart() { }

        /// <summary>
        /// The file path of the biofile to be processed.
        /// </summary>
        public string FilePath { get; set; }
    }
}
