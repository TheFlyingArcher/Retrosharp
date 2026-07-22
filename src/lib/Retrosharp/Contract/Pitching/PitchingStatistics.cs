namespace Retrosharp.Contract.Pitching
{
    /// <summary>
    /// Rate stats computed from <see cref="Pitching"/>'s stored counting stats, plus HR/9,
    /// HR/FB, FIP, and pitcher BABIP -- which need the extra per-event fields below, populated
    /// by <see cref="Service.Interface.IPlayerStatisticsService"/> from
    /// <see cref="GameEvent.PitcherEventAggregate"/> and <see cref="LeagueSeasonPitchingTotals"/>
    /// rather than being derivable from <see cref="Pitching"/> alone. See spec/api.md,
    /// "PitcherEventAggregate".
    /// </summary>
    public class PitchingStatistics : Pitching
    {
        /// <summary>Home runs allowed, from <see cref="GameEvent.PitcherEventAggregate"/>.</summary>
        public int HomerunsAllowed { get; set; }

        /// <summary>Fly balls allowed (any outcome), from <see cref="GameEvent.PitcherEventAggregate"/>.</summary>
        public int FlyBallsAllowed { get; set; }

        /// <summary>Batters faced counting as an official at-bat, from <see cref="GameEvent.PitcherEventAggregate"/>.</summary>
        public int AtBatsAgainst { get; set; }

        /// <summary>Sacrifice flies allowed, from <see cref="GameEvent.PitcherEventAggregate"/>.</summary>
        public int SacrificeFliesAgainst { get; set; }

        /// <summary>
        /// FIP's league-normalizing constant, from <see cref="LeagueSeasonPitchingTotals"/>. Null
        /// when it could not be computed (for example, zero league innings pitched).
        /// </summary>
        public double? FipConstant { get; set; }

        /// <summary>The league code the applied <see cref="FipConstant"/> came from (or a blend of two, for a combined total spanning leagues).</summary>
        public string? FipConstantLeagueCode { get; set; }

        /// <summary>The season year the applied <see cref="FipConstant"/> came from.</summary>
        public short? FipConstantSeasonYear { get; set; }
        /// <summary>
        /// Real innings pitched. <see cref="Pitching.InningsPitched"/> stores total outs
        /// recorded, not decimal innings -- see spec/api.md.
        /// </summary>
        public double InningsPitchedDecimal => InningsPitched / 3.0;

        /// <summary>
        /// Innings pitched in conventional baseball notation (for example, "6.1" meaning
        /// six and one third innings), derived from the same outs-recorded value.
        /// </summary>
        public string InningsPitchedDisplay => $"{InningsPitched / 3}.{InningsPitched % 3}";

        /// <summary>
        /// Earned run average: earned runs allowed per nine innings.
        /// </summary>
        public float Era => InningsPitched > 0 ? (float)(9 * EarnedRuns / InningsPitchedDecimal) : 0f;

        /// <summary>
        /// Walks plus hits per inning pitched.
        /// </summary>
        public float Whip => InningsPitched > 0 ? (float)((Hits + BaseOnBalls) / InningsPitchedDecimal) : 0f;

        /// <summary>
        /// Strikeouts per nine innings.
        /// </summary>
        public float StrikeoutsPerNine => InningsPitched > 0 ? (float)(9 * Strikeouts / InningsPitchedDecimal) : 0f;

        /// <summary>
        /// Walks per nine innings.
        /// </summary>
        public float WalksPerNine => InningsPitched > 0 ? (float)(9 * BaseOnBalls / InningsPitchedDecimal) : 0f;

        /// <summary>
        /// Home runs allowed per nine innings.
        /// </summary>
        public float HomeRunsPerNine => InningsPitched > 0 ? (float)(9 * HomerunsAllowed / InningsPitchedDecimal) : 0f;

        /// <summary>
        /// Home runs allowed per fly ball allowed.
        /// </summary>
        public float HomeRunsPerFlyBall => FlyBallsAllowed > 0 ? (float)HomerunsAllowed / FlyBallsAllowed : 0f;

        /// <summary>
        /// Fielding-independent pitching: (13*HR + 3*(BB+HBP) - 2*K) / IP, plus the league's
        /// normalizing constant (<see cref="FipConstant"/>, 0 when not available).
        /// </summary>
        public float Fip => InningsPitched > 0
            ? (float)((13 * HomerunsAllowed + 3 * (BaseOnBalls + HitBatsmen) - 2 * Strikeouts) / InningsPitchedDecimal + (FipConstant ?? 0))
            : 0f;

        /// <summary>
        /// Batting average on balls in play against this pitcher: (H - HR) / (AB - K - HR + SF).
        /// </summary>
        public float BattingAverageOnBallsInPlay
        {
            get
            {
                var denominator = AtBatsAgainst - Strikeouts - HomerunsAllowed + SacrificeFliesAgainst;
                var numerator = Hits - HomerunsAllowed;

                return denominator > 0 ? (float)numerator / denominator : 0f;
            }
        }
    }
}
