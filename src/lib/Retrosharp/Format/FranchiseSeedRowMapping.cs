using System;

using CsvHelper.Configuration;

namespace Retrosharp.Format
{
    /// <summary>
    /// Maps docs/csv/franchises.csv's column order to <see cref="FranchiseSeedRow"/>.
    /// The file has no header row.
    /// </summary>
    public class FranchiseSeedRowMapping : ClassMap<FranchiseSeedRow>
    {
        public FranchiseSeedRowMapping()
        {
            Map(m => m.FranchiseIdentifier).Index(0);
            Map(m => m.FranchiseCode).Index(1);
            Map(m => m.LeagueCode).Index(2);
            Map(m => m.DivisionCode).Index(3).Convert(c =>
            {
                string? value = c.Row[3];
                return string.IsNullOrWhiteSpace(value) ? null : value;
            });
            Map(m => m.FranchiseLocation).Index(4);
            Map(m => m.Nickname).Index(5);
            Map(m => m.AlternateNickname).Index(6).Convert(c =>
            {
                string? value = c.Row[6];
                return string.IsNullOrWhiteSpace(value) ? null : value;
            });
            Map(m => m.FranchiseStart).Index(7);
            Map(m => m.FranchiseEnd).Index(8).Convert(c =>
            {
                string? value = c.Row[8];
                return DateTime.TryParse(value, out var parsed) ? parsed : (DateTime?)null;
            });
            Map(m => m.PlayingCity).Index(9);
            Map(m => m.PlayingState).Index(10);
        }
    }
}
