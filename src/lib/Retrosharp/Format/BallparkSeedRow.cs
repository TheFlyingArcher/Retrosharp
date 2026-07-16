using System;

namespace Retrosharp.Format
{
    /// <summary>
    /// Represents one row of Retrosheet's ballpark reference data
    /// (docs/csv/ballparks.csv, sourced from Retrosheet's ballparks.zip). See spec/seed-data.md.
    /// </summary>
    public class BallparkSeedRow
    {
        public string SiteCode { get; set; }

        public string? ParkName { get; set; }

        public string City { get; set; }

        public string? StateProvinceCountry { get; set; }

        public DateTime? FirstGame { get; set; }

        public DateTime? LastGame { get; set; }
    }
}
