using System;
using System.Collections.Generic;
using System.Text;

using CsvHelper.Configuration;

namespace Retrosharp.Format
{
    /// <summary>
    /// Maps Retrosheet's newer-format biofile (biofile0.csv) columns, per spec/person.md.
    /// Column order confirmed against a real biofile0.csv: id, lastname, usename, fullname,
    /// birthdate, birthcity, birthstate, birthcountry, deathdate, deathcity, deathstate,
    /// deathcountry, cemetery, cem_city, cem_state, cem_ctry, cem_note, birthname, altname,
    /// debut_p, last_p, debut_c, last_c, debut_m, last_m, debut_u, last_u, bats, throws,
    /// height, weight, HOF.
    /// </summary>
    public class BioFileMapping : ClassMap<BioFile>
    {
        public BioFileMapping()
        {
            Map(m => m.RetrosheetId).Index(0);
            Map(m => m.LastName).Index(1);
            Map(m => m.UseName).Index(2);
            Map(m => m.FullName).Index(3);
            Map(m => m.BirthDate).Index(4).Convert(c => RetrosheetDateParser.Parse(c.Row[4]));
            Map(m => m.BirthCity).Index(5);
            Map(m => m.BirthState).Index(6);
            Map(m => m.BirthCountry).Index(7);
            Map(m => m.DeathDate).Index(8).Convert(c => RetrosheetDateParser.Parse(c.Row[8]));
            Map(m => m.DeathCity).Index(9);
            Map(m => m.DeathState).Index(10);
            Map(m => m.DeathCountry).Index(11);
            Map(m => m.CemetaryName).Index(12);
            Map(m => m.CemetaryCity).Index(13);
            Map(m => m.CemetaryState).Index(14);
            Map(m => m.CemetaryCountry).Index(15);
            Map(m => m.CemetaryNote).Index(16);
            Map(m => m.BirthName).Index(17);
            Map(m => m.AlternateName).Index(18);
            Map(m => m.PlayerDebut).Index(19).Convert(c => RetrosheetDateParser.Parse(c.Row[19]));
            Map(m => m.PlayerFinalGame).Index(20).Convert(c => RetrosheetDateParser.Parse(c.Row[20]));
            Map(m => m.CoachDebut).Index(21).Convert(c => RetrosheetDateParser.Parse(c.Row[21]));
            Map(m => m.FinalCoachGame).Index(22).Convert(c => RetrosheetDateParser.Parse(c.Row[22]));
            Map(m => m.ManagerDebut).Index(23).Convert(c => RetrosheetDateParser.Parse(c.Row[23]));
            Map(m => m.FinalManagerGame).Index(24).Convert(c => RetrosheetDateParser.Parse(c.Row[24]));
            Map(m => m.UmpireDebut).Index(25).Convert(c => RetrosheetDateParser.Parse(c.Row[25]));
            Map(m => m.FinalUmpireGame).Index(26).Convert(c => RetrosheetDateParser.Parse(c.Row[26]));
            Map(m => m.Bats).Index(27);
            Map(m => m.Throws).Index(28);
            Map(m => m.Height).Index(29);
            Map(m => m.Weight).Index(30);
            Map(m => m.InHallOfFame).Index(31).Convert(c => c.Row[31] == "HOF");
        }
    }
}
