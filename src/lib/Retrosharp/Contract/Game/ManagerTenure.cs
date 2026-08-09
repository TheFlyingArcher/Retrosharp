using System;

using PersonEntity = Retrosharp.Contract.Person.Person;

namespace Retrosharp.Contract.Game
{
    /// <summary>
    /// One manager's tenure with a franchise for a stretch of consecutive games within a season
    /// -- a season with a mid-season managerial change produces multiple entries, one per
    /// manager, in chronological order. See spec/api.md, "GET /teams/{id}/managers".
    /// </summary>
    public class ManagerTenure
    {
        public PersonEntity Manager { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }
    }
}
