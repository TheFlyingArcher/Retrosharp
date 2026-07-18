using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace Retrosharp.Format
{
    public class GameLog : RetrosheetFile
    {
        /// <summary>
        /// Date in the form "yyyymmdd"
        /// </summary>
        public DateTime GameDate { get; set; }

        /// <summary>
        /// Number of game:
        ///  "0" -- a single game
        ///  "1" -- the first game of a double (or triple) header
        ///         including seperate admission doubleheaders
        ///  "2"-- the second game of a double (or triple) header
        ///         including seperate admission doubleheaders
        ///  "3"-- the third game of a triple-header
        ///  "A" -- the first game of a double-header involving 3 teams
        ///  "B" -- the second game of a double-header involving 3 teams
        /// </summary>
        public char GameNumber { get; set; }

        /// <summary>
        /// Day of week  ("Sun","Mon","Tue","Wed","Thu","Fri","Sat")
        /// </summary>
        public string DayOfWeek { get; set; }

        /// <summary>
        /// Visiting team code
        /// </summary>
        public string VisitorTeamCode { get; set; }

        /// <summary>
        /// Visiting team's league
        /// </summary>
        public string VisitorLeague { get; set; }

        /// <summary>
        /// Visiting team's current game number. I.E. the 57th game played for the Rays
        /// </summary>
        public int VisitorGameNumber { get; set; }

        /// <summary>
        /// Home team code
        /// </summary>
        public string HomeTeamCode { get; set; }

        /// <summary>
        /// Home team league code
        /// </summary>
        public string HomeLeague { get; set; }

        /// <summary>
        /// Home team's current game number. I.E. the 55th game played for the Padres
        /// </summary>
        public int HomeGameNumber { get; set; }

        /// <summary>
        /// The visitor's score
        /// </summary
        public int VisitorScore { get; set; }

        /// <summary>
        /// The home team's score
        /// </summary>
        public int HomeScore { get; set; }

        /// <summary>
        /// Length of game in outs (unquoted). A full 9-inning game would
        /// have a 54 in this field. If the home team won without batting
        /// in the bottom of the ninth, this field would contain a 51.
        /// 
        /// During COVID years, doubleheader games were 7-innings
        /// Home win: 39
        /// Visitor win: 42
        /// </summary>
        public int GameLengthOuts { get; set; }

        /// <summary>
        /// Day/night indicator ("D" or "N")
        /// </summary>
        public char DayOrNight { get; set; }

        /// <summary>
        /// Completion information. If the game was completed at a later date
        /// (either due to a suspension or an upheld protest) this field will include:
        /// "yyyymmdd,park,vs,hs,len"
        /// </summary>
        public string CompletionInfo { get; set; }

        /// <summary>
        /// Forfeit information:
        /// "V" -- the game was forfeited to the visiting team
        /// "H" -- the game was forfeited to the home team
        /// "T" -- the game was ruled a no-decision
        /// </summary>
        public string ForfeitInfo { get; set; }

        /// <summary>
        /// Protest information:
        /// "P" -- the game was protested by an unidentified team
        /// "V" -- a disallowed protest was made by the visiting team
        /// "H" -- a disallowed protest was made by the home team
        /// "X" -- an upheld protest was made by the visiting team
        /// "Y" -- an upheld protest was made by the home team
        /// Note: two of these last four codes can appear in the field (if both teams protested the game).
        /// </summary>
        public string ProtestInfo { get; set; }

        public string ParkCode { get; set; }

        /// <summary>
        /// Attendance. Occasionally missing for real games (for example, a suspended game
        /// completed at a later date with no recorded attendance figure).
        /// </summary>
        public int? GameAttendance { get; set; }

        public int GameLengthMinutes { get; set; }

        public string VisitorScoreLine { get; set; }

        public string HomeScoreLine { get; set; }

        //21-37: Visitor's Offensive stats
        public GameLogHittingStatistics VisitorHitting { get; set; }

        //38-42: Visitor's Pitching stats
        public GameLogPitchingStatistics VisitorPitching { get; set; }

        //43-48: Visitor's defensive stats
        public GameLogFieldingStatistics VisitorFielding { get; set; }

        //48-65: Home's offensive stats
        public GameLogHittingStatistics HomeHitting { get; set; }

        //66-70: Home's pitching stats
        public GameLogPitchingStatistics HomePitching { get; set; }

        //71-76: Home's defensive stats
        public GameLogFieldingStatistics HomeFielding { get; set; }

        public string UmpireHomeId { get; set; }

        public string UmpireHomeName { get; set; }

        public string UmpireFirstId { get; set; }

        public string UmpireFirstName { get; set; }

        public string UmpireSecondId { get; set; }

        public string UmpireSecondName { get; set; }

        public string UmpireThirdId { get; set; }

        public string UmpireThirdName { get; set; }

        public string? UmpireLeftId { get; set; }

        public string? UmpireLeftName { get; set; }

        public string? UmpireRightId { get; set; }

        public string? UmpireRightName { get; set; }

        public string VisitorManagerId { get; set; }

        public string VisitorManagerName { get; set; }

        public string HomeManagerId { get; set; }

        public string HomeManagerName { get; set; }

        public string WinningPitcherId { get; set; }

        public string WinningPitcherName { get; set; }

        public string LosingPitcherId { get; set; }

        public string LosingPitcherName { get; set; }

        public string? SavingPitcherId { get; set; }

        public string? SavingPitcherName { get; set; }

        public string? GameWinningPlayerId { get; set; }

        public string? GameWinningPlayerName { get; set; }

        public string VisitorStartingPitcherId { get; set; }

        public string VisitorStartingPitcherName { get; set; }

        public string HomeStartingPitcherId { get; set; }

        public string HomeStartingPitcherName { get; set; }

        // 105-131: Visitor Lineup
        public GameLineup VisitorStartingLineup { get; set; }

        // 132-158: Home Lineup
        public GameLineup HomeStartingLineup { get; set; }

        /// <summary>
        /// Additional information.  This is a grab-bag of informational
        /// items that might not warrant a field on their own.
        /// </summary>
        public string AdditionalInformation { get; set; }

        public char AcquisitionInfo { get; set; }
    }

    public sealed class GameLogHittingStatistics
    {
        public int AtBats { get; set; }

        public int Hits { get; set; }

        public int Doubles { get; set; }

        public int Triples { get; set; }

        public int Homeruns { get; set; }

        public int RunsBattedIn { get; set; }

        public int SacrificeHits { get; set; }

        public int SacrificeFlies { get; set; }

        public int TimesHitByPitch { get; set; }

        public int BasesOnBalls { get; set; }

        public int IntentionalBasesOnBalls { get; set; }

        public int Strikeouts { get; set; }

        public int StolenBases { get; set; }

        public int TimesCaughtStealing { get; set; }

        public int TimesGidp { get; set; }

        public int TimesCatchersInterference { get; set; }

        public int LeftOnBase { get; set; }
    }

    public sealed class GameLogPitchingStatistics
    {
        public int PitchersUsed { get; set; }

        public int EarnedRuns { get; set; }

        public int TeamEarnedRuns { get; set; }

        public int WildPitches { get; set; }

        public int Balks { get; set; }
    }

    public sealed class GameLogFieldingStatistics
    {
        public int Putouts { get; set; }

        public int Assists { get; set; }

        public int Errors { get; set; }

        public int PassedBalls { get; set; }

        public int DoublePlays { get; set; }

        public int TriplePlays { get; set; }
    }

    public sealed class GameLineupBatter
    {
        public string PlayerId { get; set; }

        public string PlayerName { get; set; }

        public string PlayerPosition { get; set; }
    }

    public sealed class GameLineup
    {
        public GameLineupBatter LeadoffBatter { get; set; }

        public GameLineupBatter SecondBatter { get; set; }

        public GameLineupBatter ThirdBatter { get; set; }

        public GameLineupBatter CleanupBatter { get; set; }

        public GameLineupBatter FifthBatter { get; set; }

        public GameLineupBatter SixthBatter { get; set; }

        public GameLineupBatter SeventhBatter { get; set; }

        public GameLineupBatter EighthBatter { get; set; }

        public GameLineupBatter NinthBatter { get; set; }
    }
}