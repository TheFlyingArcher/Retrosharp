using System;

namespace Retrosharp.Contract.Game
{
    /// <summary>
    /// Represents a baseball game.
    /// </summary>
    public class Game : Entity
    {
        /// <summary>
        /// Date the game was played.
        /// </summary>
        public DateTime GameDate { get; set; }

        /// <summary>
        /// Game number indicator (0 for single game, 1 for first of doubleheader, etc.).
        /// </summary>
        public byte GameNumber { get; set; }

        /// <summary>
        /// Day of the week the game was played (e.g., "Mon", "Wed").
        /// </summary>
        public string GameWeekDay { get; set; }

        /// <summary>
        /// Day/night indicator ("D" for day, "N" for night).
        /// </summary>
        public string GameDayNight { get; set; }

        /// <summary>
        /// Foreign key to the visiting franchise.
        /// </summary>
        public int VisitorFranchiseId { get; set; }

        /// <summary>
        /// Game number for the visiting team in their season.
        /// </summary>
        public int VisitorGameNumber { get; set; }

        /// <summary>
        /// Runs scored by the visiting team.
        /// </summary>
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
        public string VisitorLineScore { get; set; }

        /// <summary>
        /// Foreign key to the visiting team's manager.
        /// </summary>
        public int VisitorManagerId { get; set; }

        /// <summary>
        /// Foreign key to the home franchise.
        /// </summary>
        public int HomeFranchiseId { get; set; }

        /// <summary>
        /// Game number for the home team in their season.
        /// </summary>
        public int HomeGameNumber { get; set; }

        /// <summary>
        /// Runs scored by the home team.
        /// </summary>
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
        public string HomeLineScore { get; set; }

        /// <summary>
        /// Foreign key to the home team's manager.
        /// </summary>
        public int HomeManagerId { get; set; }

        /// <summary>
        /// Foreign key to the ballpark where the game was played.
        /// </summary>
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
        public int? UmpireHomeId { get; set; }

        /// <summary>
        /// Foreign key to the first base umpire.
        /// </summary>
        public int? UmpireFirstId { get; set; }

        /// <summary>
        /// Foreign key to the second base umpire.
        /// </summary>
        public int? UmpireSecondId { get; set; }

        /// <summary>
        /// Foreign key to the third base umpire.
        /// </summary>
        public int? UmpireThirdId { get; set; }

        /// <summary>
        /// Foreign key to the left field umpire.
        /// </summary>
        public int? UmpireLeftId { get; set; }

        /// <summary>
        /// Foreign key to the right field umpire.
        /// </summary>
        public int? UmpireRightId { get; set; }

        /// <summary>
        /// Foreign key to the winning pitcher.
        /// </summary>
        public int? WinningPitcherId { get; set; }

        /// <summary>
        /// Foreign key to the losing pitcher.
        /// </summary>
        public int? LosingPitcherId { get; set; }

        /// <summary>
        /// Foreign key to the saving pitcher.
        /// </summary>
        public int? SavingPitcherId { get; set; }

        /// <summary>
        /// Foreign key to the game-winning batter.
        /// </summary>
        public int? GameWinningBatterId { get; set; }

        /// <summary>
        /// Additional notes about the game.
        /// </summary>
        public string GameNotes { get; set; }
    }
}
