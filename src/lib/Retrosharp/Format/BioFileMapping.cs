using System;
using System.Collections.Generic;
using System.Text;

using CsvHelper.Configuration;

namespace Retrosharp.Format
{
    public class BioFileMapping : ClassMap<BioFile>
    {
        public BioFileMapping()
        {
            Map(m => m.RetrosheetId).Index(0);
            Map(m => m.LastName).Index(1);
            Map(m => m.FullName).Index(2);
            Map(m => m.BirthDate)
                .Index(3)
                .Convert(c =>
                {
                    string? date = c.Row[3];
                    bool validDate = DateTime.TryParse(date, out DateTime parsedDate);

                    return validDate ? parsedDate : null;
                });
            Map(m => m.BirthCity).Index(4);
            Map(m => m.BirthState).Index(5);
            Map(m => m.BirthCountry).Index(6);
            Map(m => m.DeathDate)
                .Index(7)
                .Convert(c =>
                {
                    string? date = c.Row[7];
                    bool validDate = DateTime.TryParse(date, out DateTime parsedDate);

                    return validDate ? parsedDate : null;
                });
            Map(m => m.DeathCity).Index(8);
            Map(m => m.DeathState).Index(9);
            Map(m => m.DeathCountry).Index(10);
            Map(m => m.CemetaryName).Index(11);
            Map(m => m.CemetaryCity).Index(12);
            Map(m => m.CemetaryState).Index(13);
            Map(m => m.CemetaryCountry).Index(14);
            Map(m => m.CemetaryNote).Index(15);
            Map(m => m.BirthName).Index(16);
            Map(m => m.AlternateName).Index(17);
            Map(m => m.PlayerDebut)
                .Index(18).Convert(c =>
                {
                    string? date = c.Row[18];
                    bool validDate = DateTime.TryParse(date, out DateTime parsedDate);

                    return validDate ? parsedDate : null;
                });
            Map(m => m.PlayerFinalGame)
                .Index(19)
                .Convert(c =>
                {
                    string? date = c.Row[19];
                    bool validDate = DateTime.TryParse(date, out DateTime parsedDate);

                    return validDate ? parsedDate : null;
                });
            Map(m => m.CoachDebut)
                .Index(20).Convert(c =>
                {
                    string? date = c.Row[20];
                    bool validDate = DateTime.TryParse(date, out DateTime parsedDate);

                    return validDate ? parsedDate : null;
                });
            Map(m => m.FinalCoachGame)
                .Index(21).Convert(c =>
                {
                    string? date = c.Row[21];
                    bool validDate = DateTime.TryParse(date, out DateTime parsedDate);

                    return validDate ? parsedDate : null;
                });
            Map(m => m.ManagerDebut).Index(22).Convert(c =>
            {
                string? date = c.Row[22];
                bool validDate = DateTime.TryParse(date, out DateTime parsedDate);

                return validDate ? parsedDate : null;
            });
            Map(m => m.FinalManagerGame).Index(23).Convert(c =>
            {
                string? date = c.Row[23];
                bool validDate = DateTime.TryParse(date, out DateTime parsedDate);

                return validDate ? parsedDate : null;
            });
            Map(m => m.UmpireDebut).Index(24).Convert(c =>
            {
                string? date = c.Row[24];
                bool validDate = DateTime.TryParse(date, out DateTime parsedDate);

                return validDate ? parsedDate : null;
            });
            Map(m => m.FinalUmpireGame).Index(25).Convert(c =>
            {
                string? date = c.Row[35];
                bool validDate = DateTime.TryParse(date, out DateTime parsedDate);

                return validDate ? parsedDate : null;
            });
            Map(m => m.Bats).Index(26);
            Map(m => m.Throws).Index(27);
            Map(m => m.Height).Index(28);
            Map(m => m.Weight).Index(29);
            Map(m => m.InHallOfFame)
                .Index(30)
                .Convert(c =>
                {
                    string? s = c.Row[30];
                    if (s != null && s.StartsWith("hof", StringComparison.InvariantCultureIgnoreCase))
                        return true;

                    return false;
                });
        }
    }
}
