using System;
using System.Collections.Generic;

using BallparkEntity = Retrosharp.Contract.Ballpark.Ballpark;
using PersonEntity = Retrosharp.Contract.Person.Person;

namespace Retrosharp.Contract.Game
{
    /// <summary>
    /// One game's summary: final score, both teams' box-score totals, both starting lineups,
    /// decisions, umpires, and ballpark -- everything <c>GET /games/{gameId}</c> needs, already
    /// resolved to identity (no bare foreign keys). See spec/api.md.
    /// </summary>
    public class GameSummary
    {
        public int Id { get; set; }

        public DateTime GameDate { get; set; }

        /// <summary>
        /// 0 for a single game, 1/2 for the first/second game of a doubleheader.
        /// </summary>
        public byte GameNumber { get; set; }

        public string GameDayNight { get; set; }

        public short? GameLengthMinutes { get; set; }

        public int? ParkAttendance { get; set; }

        public string GameNotes { get; set; }

        public BallparkEntity Ballpark { get; set; }

        public GameTeamBoxScore HomeTeam { get; set; }

        public GameTeamBoxScore VisitorTeam { get; set; }

        public IReadOnlyList<GameLineupEntry> HomeLineup { get; set; }

        public IReadOnlyList<GameLineupEntry> VisitorLineup { get; set; }

        public PersonEntity WinningPitcher { get; set; }

        public PersonEntity LosingPitcher { get; set; }

        public PersonEntity SavingPitcher { get; set; }

        public PersonEntity GameWinningBatter { get; set; }

        public PersonEntity UmpireHome { get; set; }

        public PersonEntity UmpireFirst { get; set; }

        public PersonEntity UmpireSecond { get; set; }

        public PersonEntity UmpireThird { get; set; }

        public PersonEntity UmpireLeft { get; set; }

        public PersonEntity UmpireRight { get; set; }
    }

    /// <summary>
    /// One team's box score for one game -- final score plus the Game Log Parser's raw
    /// team-level totals (see <see cref="GameBattingStatistics"/>/<see cref="GamePitchingStatistics"/>/
    /// <see cref="GameFieldingStatistics"/>), already fetched via their existing
    /// <c>GetByGameIdAsync</c> repository methods.
    /// </summary>
    public class GameTeamBoxScore
    {
        public int FranchiseId { get; set; }

        public string FranchiseCode { get; set; }

        public string FranchiseName { get; set; }

        public bool IsHome { get; set; }

        public byte Runs { get; set; }

        public byte? Hits { get; set; }

        public byte? Errors { get; set; }

        public GameBattingStatistics Batting { get; set; }

        public GamePitchingStatistics Pitching { get; set; }

        public GameFieldingStatistics Fielding { get; set; }
    }

    /// <summary>
    /// One starting lineup slot, with the batter resolved to identity.
    /// </summary>
    public class GameLineupEntry
    {
        public byte LineupOrder { get; set; }

        public PersonEntity Batter { get; set; }

        /// <summary>
        /// Defensive position played (e.g., "1B", "SS", "CF").
        /// </summary>
        public string Position { get; set; }
    }
}
