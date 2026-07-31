namespace Retrosharp.UI.Api.Models
{
    /// <summary>
    /// A game's summary: final score, both teams' box-score totals, both starting lineups,
    /// decisions, umpires, and ballpark. See spec/api.md, "GET /games/{gameId}".
    /// </summary>
    public class GameSummaryResponse
    {
        public int Id { get; set; }

        public DateTime GameDate { get; set; }

        /// <summary>
        /// 0 for a single game, 1/2 for the first/second game of a doubleheader.
        /// </summary>
        public byte GameNumber { get; set; }

        public string? GameDayNight { get; set; }

        public short? GameLengthMinutes { get; set; }

        public int? ParkAttendance { get; set; }

        public string? GameNotes { get; set; }

        public string? BallparkName { get; set; }

        public string? BallparkCity { get; set; }

        public GameTeamBoxScoreResponse Home { get; set; } = new();

        public GameTeamBoxScoreResponse Visitor { get; set; } = new();

        public IEnumerable<GameLineupEntryResponse> HomeLineup { get; set; } = Array.Empty<GameLineupEntryResponse>();

        public IEnumerable<GameLineupEntryResponse> VisitorLineup { get; set; } = Array.Empty<GameLineupEntryResponse>();

        public PlayerSearchResult? WinningPitcher { get; set; }

        public PlayerSearchResult? LosingPitcher { get; set; }

        public PlayerSearchResult? SavingPitcher { get; set; }

        public PlayerSearchResult? GameWinningBatter { get; set; }

        public PlayerSearchResult? UmpireHome { get; set; }

        public PlayerSearchResult? UmpireFirst { get; set; }

        public PlayerSearchResult? UmpireSecond { get; set; }

        public PlayerSearchResult? UmpireThird { get; set; }

        public PlayerSearchResult? UmpireLeft { get; set; }

        public PlayerSearchResult? UmpireRight { get; set; }
    }

    /// <summary>
    /// One team's box score for one game. <see cref="Batting"/>/<see cref="Pitching"/>/
    /// <see cref="Fielding"/> field names deliberately match
    /// <c>GameBattingStatistics</c>/<c>GamePitchingStatistics</c>/<c>GameFieldingStatistics</c>
    /// exactly (e.g. <c>Hit</c>, not <c>Hits</c>) so Mapster's <c>Adapt&lt;T&gt;()</c> maps them
    /// with no manual fixup -- unlike <c>BattingLine</c>/<c>PitchingLine</c>/<c>FieldingLine</c>
    /// (built for the season/career shape), which caused a silent zeroed-field bug in Step 7e
    /// when reused for team-game data with different source field names.
    /// </summary>
    public class GameTeamBoxScoreResponse
    {
        public int FranchiseId { get; set; }

        public string FranchiseCode { get; set; } = string.Empty;

        public string FranchiseName { get; set; } = string.Empty;

        public bool IsHome { get; set; }

        public byte Runs { get; set; }

        public byte? Hits { get; set; }

        public byte? Errors { get; set; }

        public GameBoxScoreBatting? Batting { get; set; }

        public GameBoxScorePitching? Pitching { get; set; }

        public GameBoxScoreFielding? Fielding { get; set; }
    }

    public class GameBoxScoreBatting
    {
        public short PlateAppearances { get; set; }

        public short AtBats { get; set; }

        public short Hit { get; set; }

        public short Doubles { get; set; }

        public short Triples { get; set; }

        public short Homeruns { get; set; }

        public short RunsBattedIn { get; set; }

        public short BaseOnBalls { get; set; }

        public short Strikeouts { get; set; }

        public short SacrificeFlies { get; set; }

        public short SacrificeBunts { get; set; }

        public short IntentionalBb { get; set; }

        public short HitByPitches { get; set; }

        public short StolenBases { get; set; }

        public short TimesCaughtStealing { get; set; }

        public short Runs { get; set; }

        public short GroundedIntoDoublePlay { get; set; }
    }

    public class GameBoxScorePitching
    {
        public byte PitchersUsed { get; set; }

        public short IndividualEarnedRuns { get; set; }

        public short TeamEarnedRuns { get; set; }

        public byte WildPitches { get; set; }

        public byte Balks { get; set; }
    }

    public class GameBoxScoreFielding
    {
        public short Putouts { get; set; }

        public short Assists { get; set; }

        public short Errors { get; set; }

        public byte PassedBalls { get; set; }

        public byte DoublePlays { get; set; }

        public byte TriplePlays { get; set; }
    }

    /// <summary>
    /// One starting lineup slot, with the batter resolved to identity.
    /// </summary>
    public class GameLineupEntryResponse
    {
        public byte LineupOrder { get; set; }

        public PlayerSearchResult? Batter { get; set; }

        /// <summary>
        /// Defensive position played (e.g., "1B", "SS", "CF").
        /// </summary>
        public string? Position { get; set; }
    }
}
