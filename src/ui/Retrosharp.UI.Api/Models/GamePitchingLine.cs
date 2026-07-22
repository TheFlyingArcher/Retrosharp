namespace Retrosharp.UI.Api.Models
{
    /// <summary>
    /// One game's pitching line for a player. <see cref="EarnedRuns"/> is the independently-
    /// computed figure, not necessarily the authoritative per-game "data,er,..." record value --
    /// see spec/api.md. See spec/api.md, "GET /players/{id}/games".
    /// </summary>
    public class GamePitchingLine
    {
        public int GameId { get; set; }

        public DateTime GameDate { get; set; }

        public bool IsHome { get; set; }

        public string FranchiseCode { get; set; } = string.Empty;

        public string OpponentFranchiseCode { get; set; } = string.Empty;

        public short GamesStarted { get; set; }

        public short GamesFinished { get; set; }

        public short CompleteGames { get; set; }

        public short Shutouts { get; set; }

        public short Saves { get; set; }

        public string InningsPitchedDisplay { get; set; } = string.Empty;

        public short Hits { get; set; }

        public short Runs { get; set; }

        public short EarnedRuns { get; set; }

        public short BaseOnBalls { get; set; }

        public short Strikeouts { get; set; }

        public short IntentionalBb { get; set; }

        public short HitBatsmen { get; set; }

        public short Balks { get; set; }

        public short WildPitches { get; set; }
    }
}
