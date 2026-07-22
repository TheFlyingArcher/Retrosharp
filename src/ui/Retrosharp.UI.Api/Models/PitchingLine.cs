namespace Retrosharp.UI.Api.Models
{
    /// <summary>
    /// One pitching statistics row (or a combined total across rows).
    /// </summary>
    public class PitchingLine
    {
        public string? FranchiseCode { get; set; }

        public string? FranchiseName { get; set; }

        public short? SeasonYear { get; set; }

        public short GamesPitched { get; set; }

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

        public float Era { get; set; }

        public float Whip { get; set; }

        public float StrikeoutsPerNine { get; set; }

        public float WalksPerNine { get; set; }

        public float HomeRunsPerNine { get; set; }

        public float HomeRunsPerFlyBall { get; set; }

        public float Fip { get; set; }

        public float BattingAverageOnBallsInPlay { get; set; }

        /// <summary>
        /// FIP's league-normalizing constant actually applied, and the league/season it was
        /// derived from -- included for transparency (see spec/api.md). Null when it could not
        /// be computed (for example, no league data available for this franchise).
        /// </summary>
        public double? FipConstant { get; set; }

        public string? FipConstantLeagueCode { get; set; }

        public short? FipConstantSeasonYear { get; set; }
    }
}
