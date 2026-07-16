using System;

using CsvHelper.Configuration;

namespace Retrosharp.Format
{
    /// <summary>
    /// Maps docs/csv/ballparks.csv's columns (PARKID,NAME,AKA,CITY,STATE,START,END,LEAGUE,NOTES)
    /// to <see cref="BallparkSeedRow"/>. AKA, LEAGUE, and NOTES are not currently modeled by
    /// <c>Ballpark</c> and are intentionally not mapped. The file has a header row.
    /// </summary>
    public class BallparkSeedRowMapping : ClassMap<BallparkSeedRow>
    {
        public BallparkSeedRowMapping()
        {
            Map(m => m.SiteCode).Index(0);
            Map(m => m.ParkName).Index(1).Convert(c =>
            {
                string? value = c.Row[1];
                return string.IsNullOrWhiteSpace(value) ? null : value;
            });
            // Index 2 (AKA) intentionally skipped.
            Map(m => m.City).Index(3);
            Map(m => m.StateProvinceCountry).Index(4).Convert(c =>
            {
                string? value = c.Row[4];
                return string.IsNullOrWhiteSpace(value) ? null : value;
            });
            Map(m => m.FirstGame).Index(5).Convert(c =>
            {
                string? value = c.Row[5];
                return DateTime.TryParse(value, out var parsed) ? parsed : (DateTime?)null;
            });
            Map(m => m.LastGame).Index(6).Convert(c =>
            {
                string? value = c.Row[6];
                return DateTime.TryParse(value, out var parsed) ? parsed : (DateTime?)null;
            });
            // Index 7 (LEAGUE) and 8 (NOTES) intentionally skipped.
        }
    }
}
