using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Retrosharp.Data.Model
{
    /// <summary>
    /// Represents a baseball person (player, manager, umpire, coach).
    /// </summary>
    [Table("Person")]
    public class PersonModel : DbModel
    {
        /// <summary>
        /// Retrosheet unique identifier for the person.
        /// </summary>
        [Required]
        [StringLength(16)]
        public string RetroSheetId { get; set; }

        /// <summary>
        /// Person's surname.
        /// </summary>
        [StringLength(32)]
        public string Surname { get; set; }

        /// <summary>
        /// Name commonly used (typically first name or nickname).
        /// </summary>
        [StringLength(32)]
        public string UseName { get; set; }

        /// <summary>
        /// Person's full name.
        /// </summary>
        [StringLength(128)]
        public string FullName { get; set; }

        /// <summary>
        /// Date of birth.
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// City where the person was born.
        /// </summary>
        [StringLength(32)]
        public string BirthCity { get; set; }

        /// <summary>
        /// State, province, or region where the person was born.
        /// </summary>
        [StringLength(32)]
        public string BirthStateProvince { get; set; }

        /// <summary>
        /// Country where the person was born.
        /// </summary>
        [StringLength(32)]
        public string BirthCountry { get; set; }

        /// <summary>
        /// Date of death.
        /// </summary>
        public DateTime? DeathDate { get; set; }

        /// <summary>
        /// City where the person died.
        /// </summary>
        [StringLength(512)]
        public string DeathCity { get; set; }

        /// <summary>
        /// State, province, or region where the person died.
        /// </summary>
        [StringLength(32)]
        public string DeathStateProvince { get; set; }

        /// <summary>
        /// Country where the person died.
        /// </summary>
        [StringLength(32)]
        public string DeathCountry { get; set; }

        /// <summary>
        /// Name of the cemetery where the person is buried.
        /// </summary>
        [StringLength(72)]
        public string Cemetery { get; set; }

        /// <summary>
        /// City of the cemetery.
        /// </summary>
        [StringLength(32)]
        public string CemeteryCity { get; set; }

        /// <summary>
        /// State, province, or region of the cemetery.
        /// </summary>
        [StringLength(32)]
        public string CemeteryStateProv { get; set; }

        /// <summary>
        /// Country of the cemetery.
        /// </summary>
        [StringLength(32)]
        public string CemeteryCountry { get; set; }

        /// <summary>
        /// Additional notes about the cemetery.
        /// </summary>
        [StringLength(1024)]
        public string CemeteryNote { get; set; }

        /// <summary>
        /// Birth name if different from current name.
        /// </summary>
        [StringLength(128)]
        public string BirthName { get; set; }

        /// <summary>
        /// Alternate name for the person.
        /// </summary>
        [StringLength(128)]
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
        [StringLength(2)]
        public string Bats { get; set; }

        /// <summary>
        /// Throwing hand (e.g., "R" for right, "L" for left).
        /// </summary>
        [StringLength(2)]
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
        [Required]
        public bool IsHof { get; set; }

        /// <summary>
        /// Navigation property for batting statistics.
        /// </summary>
        public ICollection<BattingModel> BattingRecords { get; set; } = new List<BattingModel>();

        /// <summary>
        /// Navigation property for pitching statistics.
        /// </summary>
        public ICollection<PitchingModel> PitchingRecords { get; set; } = new List<PitchingModel>();

        /// <summary>
        /// Navigation property for games where this person was a manager (visitor).
        /// </summary>
        public ICollection<GameModel> VisitorManagerGames { get; set; } = new List<GameModel>();

        /// <summary>
        /// Navigation property for games where this person was a manager (home).
        /// </summary>
        public ICollection<GameModel> HomeManagerGames { get; set; } = new List<GameModel>();

        /// <summary>
        /// Navigation property for games where this person was home plate umpire.
        /// </summary>
        public ICollection<GameModel> UmpireHomeGames { get; set; } = new List<GameModel>();

        /// <summary>
        /// Navigation property for games where this person was first base umpire.
        /// </summary>
        public ICollection<GameModel> UmpireFirstGames { get; set; } = new List<GameModel>();

        /// <summary>
        /// Navigation property for games where this person was second base umpire.
        /// </summary>
        public ICollection<GameModel> UmpireSecondGames { get; set; } = new List<GameModel>();

        /// <summary>
        /// Navigation property for games where this person was third base umpire.
        /// </summary>
        public ICollection<GameModel> UmpireThirdGames { get; set; } = new List<GameModel>();

        /// <summary>
        /// Navigation property for games where this person was left field umpire.
        /// </summary>
        public ICollection<GameModel> UmpireLeftGames { get; set; } = new List<GameModel>();

        /// <summary>
        /// Navigation property for games where this person was right field umpire.
        /// </summary>
        public ICollection<GameModel> UmpireRightGames { get; set; } = new List<GameModel>();

        /// <summary>
        /// Navigation property for games where this person was the winning pitcher.
        /// </summary>
        public ICollection<GameModel> WinningPitcherGames { get; set; } = new List<GameModel>();

        /// <summary>
        /// Navigation property for games where this person was the losing pitcher.
        /// </summary>
        public ICollection<GameModel> LosingPitcherGames { get; set; } = new List<GameModel>();

        /// <summary>
        /// Navigation property for games where this person was the saving pitcher.
        /// </summary>
        public ICollection<GameModel> SavingPitcherGames { get; set; } = new List<GameModel>();

        /// <summary>
        /// Navigation property for games where this person was the game-winning batter.
        /// </summary>
        public ICollection<GameModel> GameWinningBatterGames { get; set; } = new List<GameModel>();

        /// <summary>
        /// Navigation property for game lineups where this person batted.
        /// </summary>
        public ICollection<GameLineupModel> GameLineups { get; set; } = new List<GameLineupModel>();
    }
}
