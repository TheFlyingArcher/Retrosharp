using System;
using System.Collections.Generic;
using System.Text;

namespace Retrosharp.Format
{
    public class BioFile : RetrosheetFile
    {
        public BioFile()
        {
            RetrosheetId = string.Empty;
            LastName = string.Empty;
            FullName = string.Empty;
        }

        public string RetrosheetId { get; set; }

        public string LastName { get; set; }

        /// <summary>
        /// Name commonly used, if different from the legal name. Blank in the source file means
        /// no alternate use-name was recorded, not a use-name of "" -- mapped to null rather than
        /// empty string. See <see cref="BioFileMapping"/>.
        /// </summary>
        public string? UseName { get; set; }

        public string FullName { get; set; }

        public DateTime? BirthDate { get; set; }

        public string? BirthCity { get; set; }

        public string? BirthState { get; set; }

        public string? BirthCountry { get; set; }

        public DateTime? DeathDate { get; set; }

        public string? DeathCity { get; set; }

        public string? DeathState { get; set; }

        public string? DeathCountry { get; set; }

        public string? CemetaryName { get; set; }

        public string? CemetaryCity { get; set; }

        public string? CemetaryState { get; set; }

        public string? CemetaryCountry { get; set; }

        public string? CemetaryNote { get; set; }

        public string? BirthName { get; set; }

        public string? AlternateName { get; set; }

        public DateTime? PlayerDebut { get; set; }

        public DateTime? PlayerFinalGame { get; set; }

        public DateTime? CoachDebut { get; set; }

        public DateTime? FinalCoachGame { get; set; }

        public DateTime? ManagerDebut { get; set; }

        public DateTime? FinalManagerGame { get; set; }

        public DateTime? UmpireDebut { get; set; }

        public DateTime? FinalUmpireGame { get; set; }

        public char? Bats { get; set; }

        public char? Throws { get; set; }

        public float? Height { get; set; }

        public float? Weight { get; set; }

        public bool InHallOfFame { get; set; }
    }
}
