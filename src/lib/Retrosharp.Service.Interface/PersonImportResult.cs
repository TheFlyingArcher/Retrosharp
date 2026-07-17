using System;

namespace Retrosharp.Service.Interface
{
    /// <summary>
    /// Reports how many Person records were added versus updated by a biofile import.
    /// </summary>
    public class PersonImportResult
    {
        public int PeopleAdded { get; set; }

        public int PeopleUpdated { get; set; }
    }
}
