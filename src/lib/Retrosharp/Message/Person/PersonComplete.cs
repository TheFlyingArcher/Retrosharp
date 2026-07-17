using System;

namespace Retrosharp.Message.Person
{
    public class PersonComplete : BaseMessage, IMessage
    {
        public PersonComplete() { }

        /// <summary>
        /// Number of people added by the import.
        /// </summary>
        public int PeopleAdded { get; set; }

        /// <summary>
        /// Number of people updated by the import.
        /// </summary>
        public int PeopleUpdated { get; set; }
    }
}
