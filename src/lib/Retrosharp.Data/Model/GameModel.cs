using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Retrosharp.Data.Model
{
    /// <summary>
    /// Represents a baseball game.
    /// </summary>
    [Table("Game")]
    public class GameModel : DbModel
    {
        /// <summary>
        /// Date the game was played.
        /// </summary>
        [Required]
        public DateTime GameDate { get; set; }

        /// <summary>
        /// GameModel number indicator (0 for single game, 1 for first of doubleheader, etc.).
        /// </summary>
        [Required]
        public byte GameNumber { get; set; }

        /// <summary>
        /// Day of the week the game was played (e.g., "Mon", "Wed").
        /// </summary>
        [StringLength(3)]
        public string GameWeekDay { get; set; }

        /// <summary>
        /// Day/night indicator ("D" for day, "N" for night).
        /// </summary>
        [StringLength(1)]
        public string GameDayNight { get; set; }

        /// <summary>
        /// Foreign key to the visiting franchise.
        /// </summary>
        [ForeignKey("VisitorFranchise")]
        [Required]
        public int VisitorFranchiseId { get; set; }

        /// <summary>
        /// GameModel number for the visiting team in their season.
        /// </summary>
        [Required]
        public int VisitorGameNumber { get; set; }

        /// <summary>
        /// Runs scored by the visiting team.
        /// </summary>
        [Required]
        public byte VisitorRuns { get; set; }

        /// <summary>
        /// Hits by the visiting team.
        /// </summary>
        public byte? VisitorHits { get; set; }

        /// <summary>
        /// Errors by the visiting team.
        /// </summary>
        public byte? VisitorErrors { get; set; }

        /// <summary>
        /// Line score for the visiting team (runs by inning).
        /// </summary>
        [StringLength(64)]
        public string VisitorLineScore { get; set; }

        /// <summary>
        /// Foreign key to the visiting team's manager.
        /// </summary>
        [ForeignKey("VisitorManager")]
        [Required]
        public int VisitorManagerId { get; set; }

        /// <summary>
        /// Foreign key to the home franchise.
        /// </summary>
        [ForeignKey("HomeFranchise")]
        [Required]
        public int HomeFranchiseId { get; set; }

        /// <summary>
        /// GameModel number for the home team in their season.
        /// </summary>
        [Required]
        public int HomeGameNumber { get; set; }

        /// <summary>
        /// Runs scored by the home team.
        /// </summary>
        [Required]
        public byte HomeTeamRuns { get; set; }

        /// <summary>
        /// Hits by the home team.
        /// </summary>
        public byte? HomeHits { get; set; }

        /// <summary>
        /// Errors by the home team.
        /// </summary>
        public byte? HomeErrors { get; set; }

        /// <summary>
        /// Line score for the home team (runs by inning).
        /// </summary>
        [StringLength(64)]
        public string HomeLineScore { get; set; }

        /// <summary>
        /// Foreign key to the home team's manager.
        /// </summary>
        [ForeignKey("HomeManager")]
        [Required]
        public int HomeManagerId { get; set; }

        /// <summary>
        /// Foreign key to the ballpark where the game was played.
        /// </summary>
        [ForeignKey("BallparkModel")]
        [Required]
        public int BallparkId { get; set; }

        /// <summary>
        /// Length of the game in minutes.
        /// </summary>
        public short? GameLengthMinutes { get; set; }

        /// <summary>
        /// Attendance at the game.
        /// </summary>
        public int? ParkAttendance { get; set; }

        /// <summary>
        /// Foreign key to the home plate umpire.
        /// </summary>
        [ForeignKey("UmpireHome")]
        public int? UmpireHomeId { get; set; }

        /// <summary>
        /// Foreign key to the first base umpire.
        /// </summary>
        [ForeignKey("UmpireFirst")]
        public int? UmpireFirstId { get; set; }

        /// <summary>
        /// Foreign key to the second base umpire.
        /// </summary>
        [ForeignKey("UmpireSecond")]
        public int? UmpireSecondId { get; set; }

        /// <summary>
        /// Foreign key to the third base umpire.
        /// </summary>
        [ForeignKey("UmpireThird")]
        public int? UmpireThirdId { get; set; }

        /// <summary>
        /// Foreign key to the left field umpire.
        /// </summary>
        [ForeignKey("UmpireLeft")]
        public int? UmpireLeftId { get; set; }

        /// <summary>
        /// Foreign key to the right field umpire.
        /// </summary>
        [ForeignKey("UmpireRight")]
        public int? UmpireRightId { get; set; }

        /// <summary>
        /// Foreign key to the winning pitcher.
        /// </summary>
        [ForeignKey("WinningPitcher")]
        public int? WinningPitcherId { get; set; }

        /// <summary>
        /// Foreign key to the losing pitcher.
        /// </summary>
        [ForeignKey("LosingPitcher")]
        public int? LosingPitcherId { get; set; }

        /// <summary>
        /// Foreign key to the saving pitcher.
        /// </summary>
        [ForeignKey("SavingPitcher")]
        public int? SavingPitcherId { get; set; }

        /// <summary>
        /// Foreign key to the game-winning batter.
        /// </summary>
        [ForeignKey("GameWinningBatter")]
        public int? GameWinningBatterId { get; set; }

        /// <summary>
        /// Additional notes about the game.
        /// </summary>
        [StringLength(2048)]
        public string GameNotes { get; set; }

        // Navigation Properties

        /// <summary>
        /// Navigation property for the visiting franchise.
        /// </summary>
        public FranchiseModel VisitorFranchise { get; set; }

        /// <summary>
        /// Navigation property for the home franchise.
        /// </summary>
        public FranchiseModel HomeFranchise { get; set; }

        /// <summary>
        /// Navigation property for the visiting team's manager.
        /// </summary>
        public PersonModel VisitorManager { get; set; }

        /// <summary>
        /// Navigation property for the home team's manager.
        /// </summary>
        public PersonModel HomeManager { get; set; }

        /// <summary>
        /// Navigation property for the ballpark.
        /// </summary>
        public BallparkModel Ballpark { get; set; }

        /// <summary>
        /// Navigation property for the home plate umpire.
        /// </summary>
        public PersonModel UmpireHome { get; set; }

        /// <summary>
        /// Navigation property for the first base umpire.
        /// </summary>
        public PersonModel UmpireFirst { get; set; }

        /// <summary>
        /// Navigation property for the second base umpire.
        /// </summary>
        public PersonModel UmpireSecond { get; set; }

        /// <summary>
        /// Navigation property for the third base umpire.
        /// </summary>
        public PersonModel UmpireThird { get; set; }

        /// <summary>
        /// Navigation property for the left field umpire.
        /// </summary>
        public PersonModel UmpireLeft { get; set; }

        /// <summary>
        /// Navigation property for the right field umpire.
        /// </summary>
        public PersonModel UmpireRight { get; set; }

        /// <summary>
        /// Navigation property for the winning pitcher.
        /// </summary>
        public PersonModel WinningPitcher { get; set; }

        /// <summary>
        /// Navigation property for the losing pitcher.
        /// </summary>
        public PersonModel LosingPitcher { get; set; }

        /// <summary>
        /// Navigation property for the saving pitcher.
        /// </summary>
        public PersonModel SavingPitcher { get; set; }

        /// <summary>
        /// Navigation property for the game-winning batter.
        /// </summary>
        public PersonModel GameWinningBatter { get; set; }

        /// <summary>
        /// Navigation property for game lineups.
        /// </summary>
        public ICollection<GameLineupModel> GameLineups { get; set; } = new List<GameLineupModel>();

        /// <summary>
        /// Navigation property for team-level batting statistics.
        /// </summary>
        public ICollection<GameBattingStatisticsModel> GameBattingStatistics { get; set; } = new List<GameBattingStatisticsModel>();

        /// <summary>
        /// Navigation property for team-level pitching statistics.
        /// </summary>
        public ICollection<GamePitchingStatisticsModel> GamePitchingStatistics { get; set; } = new List<GamePitchingStatisticsModel>();

        /// <summary>
        /// Navigation property for team-level fielding statistics.
        /// </summary>
        public ICollection<GameFieldingStatisticsModel> GameFieldingStatistics { get; set; } = new List<GameFieldingStatisticsModel>();

        /// <summary>
        /// Navigation property for play-by-play events. Populated exclusively by the Game
        /// Event Parser.
        /// </summary>
        public ICollection<GameEventModel> GameEvents { get; set; } = new List<GameEventModel>();

        /// <summary>
        /// Navigation property for in-game substitutions.
        /// </summary>
        public ICollection<GameSubstitutionModel> GameSubstitutions { get; set; } = new List<GameSubstitutionModel>();

        /// <summary>
        /// Navigation property for handedness/lineup/responsibility adjustment records.
        /// </summary>
        public ICollection<GameAdjustmentModel> GameAdjustments { get; set; } = new List<GameAdjustmentModel>();

        /// <summary>
        /// Navigation property for free-text commentary records.
        /// </summary>
        public ICollection<GameCommentModel> GameComments { get; set; } = new List<GameCommentModel>();

        /// <summary>
        /// Navigation property for this game's Game Event processing status. Null until the
        /// Game Event Parser has fully applied this game's statistics.
        /// </summary>
        public GameEventGameStatusModel GameEventGameStatus { get; set; }
    }
}
