using System;
using System.Collections.Generic;

using BallparkEntity = Retrosharp.Contract.Ballpark.Ballpark;
using PersonEntity = Retrosharp.Contract.Person.Person;
using Retrosharp.Contract.Batting;
using Retrosharp.Contract.Pitching;

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

        /// <summary>
        /// The game's local start time, from its event file's "info,starttime,..." record --
        /// null for a game with no imported event file, or whose event file had no parseable
        /// value. See spec/game-event.md, "Future Enhancement (Phase 1 gap): Game start time
        /// from info records".
        /// </summary>
        public TimeOnly? StartTimeLocal { get; set; }

        public BallparkEntity Ballpark { get; set; }

        public GameTeamBoxScore HomeTeam { get; set; }

        public GameTeamBoxScore VisitorTeam { get; set; }

        public IReadOnlyList<GameLineupEntry> HomeLineup { get; set; }

        public IReadOnlyList<GameLineupEntry> VisitorLineup { get; set; }

        public PersonEntity VisitorStartingPitcher { get; set; }

        public PersonEntity HomeStartingPitcher { get; set; }

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

        /// <summary>
        /// Inning-by-inning line score (e.g. "010000(10)0x"), as stored on <c>Game</c>.
        /// </summary>
        public string LineScore { get; set; }

        public GameBattingStatistics Batting { get; set; }

        public GamePitchingStatistics Pitching { get; set; }

        public GameFieldingStatistics Fielding { get; set; }

        /// <summary>
        /// One batting line per distinct batter who appeared for this team -- starters and
        /// substitutes alike -- derived on demand from play-by-play. Empty (not null) for a
        /// game with no imported play-by-play, the same graceful-empty convention already used
        /// for <see cref="Retrosharp.Contract.GameEvent.GamePlayByPlay"/>.
        /// </summary>
        public IReadOnlyList<GameBoxScoreBattingParticipant> Batters { get; set; } = Array.Empty<GameBoxScoreBattingParticipant>();

        /// <summary>
        /// One pitching line per distinct pitcher who appeared for this team -- starters and
        /// relievers alike. Same empty-not-null convention as <see cref="Batters"/>.
        /// </summary>
        public IReadOnlyList<GameBoxScorePitchingParticipant> Pitchers { get; set; } = Array.Empty<GameBoxScorePitchingParticipant>();
    }

    /// <summary>
    /// One batter's box-score line for one game, with identity and defensive position(s)
    /// already resolved. See spec/phase-1-build-plan.md, Step 7i.
    /// </summary>
    public class GameBoxScoreBattingParticipant
    {
        public PersonEntity Player { get; set; }

        /// <summary>
        /// Defensive position(s) played, comma-separated if more than one (e.g. moved from one
        /// position to another mid-game) -- raw Retrosheet position codes ("1"-"9", "10" DH),
        /// not display abbreviations; no code/abbreviation table exists elsewhere in this
        /// project to translate them. Null if the player has no lineup or substitution record
        /// for this game (shouldn't happen for anyone with a batting delta, but not assumed).
        /// </summary>
        public string Position { get; set; }

        public BattingDelta Stats { get; set; }
    }

    /// <summary>
    /// One pitcher's box-score line for one game, with identity already resolved. See
    /// spec/phase-1-build-plan.md, Step 7i.
    /// </summary>
    public class GameBoxScorePitchingParticipant
    {
        public PersonEntity Player { get; set; }

        public PitchingDelta Stats { get; set; }
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
