using System;

namespace Retrosharp.Contract.Ballpark
{
    /// <summary>
    /// Represents a baseball stadium/ballpark.
    /// </summary>
    public class Ballpark : Entity
    {
        /// <summary>
        /// Site code identifier for the ballpark.
        /// </summary>
        public string SiteCode { get; set; }

        /// <summary>
        /// Official name of the ballpark. Not always known for older, minor sites.
        /// </summary>
        public string? ParkName { get; set; }

        /// <summary>
        /// City where the ballpark is located.
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// State, province, or country where the ballpark is located.
        /// </summary>
        public string? StateProvinceCountry { get; set; }

        /// <summary>
        /// Date of the first game played at this ballpark. Not always known — Retrosheet's
        /// ballpark data leaves this blank for many sites, particularly older or minor ones.
        /// </summary>
        public DateTime? FirstGame { get; set; }

        /// <summary>
        /// Date of the last game played at this ballpark (if no longer in use).
        /// </summary>
        public DateTime? LastGame { get; set; }
    }
}
