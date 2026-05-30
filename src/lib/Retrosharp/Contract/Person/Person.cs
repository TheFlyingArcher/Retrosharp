using System;

namespace Retrosharp.Contract.Person
{
    /// <summary>
    /// Represents a baseball person (player, manager, umpire, coach).
    /// </summary>
    public class Person : Entity
    {
        /// <summary>
        /// Retrosheet unique identifier for the person.
        /// </summary>
        public string RetroSheetId { get; set; }

        /// <summary>
        /// Person's surname.
        /// </summary>
        public string Surname { get; set; }

        /// <summary>
        /// Name commonly used (typically first name or nickname).
        /// </summary>
        public string UseName { get; set; }

        /// <summary>
        /// Person's full name.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Date of birth.
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// City where the person was born.
        /// </summary>
        public string BirthCity { get; set; }

        /// <summary>
        /// State, province, or region where the person was born.
        /// </summary>
        public string BirthStateProvince { get; set; }

        /// <summary>
        /// Country where the person was born.
        /// </summary>
        public string BirthCountry { get; set; }

        /// <summary>
        /// Date of death.
        /// </summary>
        public DateTime? DeathDate { get; set; }

        /// <summary>
        /// City where the person died.
        /// </summary>
        public string DeathCity { get; set; }

        /// <summary>
        /// State, province, or region where the person died.
        /// </summary>
        public string DeathStateProvince { get; set; }

        /// <summary>
        /// Country where the person died.
        /// </summary>
        public string DeathCountry { get; set; }

        /// <summary>
        /// Name of the cemetery where the person is buried.
        /// </summary>
        public string Cemetery { get; set; }

        /// <summary>
        /// City of the cemetery.
        /// </summary>
        public string CemeteryCity { get; set; }

        /// <summary>
        /// State, province, or region of the cemetery.
        /// </summary>
        public string CemeteryStateProv { get; set; }

        /// <summary>
        /// Country of the cemetery.
        /// </summary>
        public string CemeteryCountry { get; set; }

        /// <summary>
        /// Additional notes about the cemetery.
        /// </summary>
        public string CemeteryNote { get; set; }

        /// <summary>
        /// Birth name if different from current name.
        /// </summary>
        public string BirthName { get; set; }

        /// <summary>
        /// Alternate name for the person.
        /// </summary>
        public string AlternateName { get; set; }

        /// <summary>
        /// Date of first player appearance.
        /// </summary>
        public DateTime? PlayerDebutDate { get; set; }

        /// <summary>
        /// Date of last player appearance.
        /// </summary>
        public DateTime? PlayerLastDate { get; set; }

        /// <summary>
        /// Date of first coaching appearance.
        /// </summary>
        public DateTime? CoachDebutDate { get; set; }

        /// <summary>
        /// Date of last coaching appearance.
        /// </summary>
        public DateTime? CoachLastDate { get; set; }

        /// <summary>
        /// Date of first managerial appearance.
        /// </summary>
        public DateTime? ManagerDebutDate { get; set; }

        /// <summary>
        /// Date of last managerial appearance.
        /// </summary>
        public DateTime? ManagerLastDate { get; set; }

        /// <summary>
        /// Date of first umpiring appearance.
        /// </summary>
        public DateTime? UmpireDebutDate { get; set; }

        /// <summary>
        /// Date of last umpiring appearance.
        /// </summary>
        public DateTime? UmpireLastDate { get; set; }

        /// <summary>
        /// Batting stance (e.g., "R" for right, "L" for left, "B" for both).
        /// </summary>
        public string Bats { get; set; }

        /// <summary>
        /// Throwing hand (e.g., "R" for right, "L" for left).
        /// </summary>
        public string Throws { get; set; }

        /// <summary>
        /// Height in inches.
        /// </summary>
        public float? Height { get; set; }

        /// <summary>
        /// Weight in pounds.
        /// </summary>
        public float? Weight { get; set; }

        /// <summary>
        /// Indicates if the person is in the Hall of Fame.
        /// </summary>
        public bool IsHof { get; set; }
    }
}
