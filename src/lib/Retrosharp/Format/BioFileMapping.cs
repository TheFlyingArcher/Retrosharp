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
            Map(m => m.UseName).Index(2).Convert(c => NullIfBlank(c.Row[2]));
            Map(m => m.FullName).Index(3);
            Map(m => m.BirthDate).Index(4).Convert(c => RetrosheetDateParser.Parse(c.Row[4]));
            Map(m => m.BirthCity).Index(5).Convert(c => NullIfBlank(c.Row[5]));
            Map(m => m.BirthState).Index(6).Convert(c => NullIfBlank(c.Row[6]));
            Map(m => m.BirthCountry).Index(7).Convert(c => NullIfBlank(c.Row[7]));
            Map(m => m.DeathDate).Index(8).Convert(c => RetrosheetDateParser.Parse(c.Row[8]));
            Map(m => m.DeathCity).Index(9).Convert(c => NullIfBlank(c.Row[9]));
            Map(m => m.DeathState).Index(10).Convert(c => NullIfBlank(c.Row[10]));
            Map(m => m.DeathCountry).Index(11).Convert(c => NullIfBlank(c.Row[11]));
            Map(m => m.CemetaryName).Index(12).Convert(c => NullIfBlank(c.Row[12]));
            Map(m => m.CemetaryCity).Index(13).Convert(c => NullIfBlank(c.Row[13]));
            Map(m => m.CemetaryState).Index(14).Convert(c => NullIfBlank(c.Row[14]));
            Map(m => m.CemetaryCountry).Index(15).Convert(c => NullIfBlank(c.Row[15]));
            Map(m => m.CemetaryNote).Index(16).Convert(c => NullIfBlank(c.Row[16]));
            Map(m => m.BirthName).Index(17).Convert(c => NullIfBlank(c.Row[17]));
            Map(m => m.AlternateName).Index(18).Convert(c => NullIfBlank(c.Row[18]));
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

        /// <summary>
        /// A blank optional field means no value was recorded, not a value of "" -- prefer null
        /// so the database stores NULL rather than wasting space (and muddying indexes/queries)
        /// with empty strings for the large fraction of biographical fields that are routinely
        /// unpopulated (most people have no death or cemetery info, most have no alternate name).
        /// </summary>
        private static string? NullIfBlank(string? raw) => string.IsNullOrEmpty(raw) ? null : raw;
    }
}
