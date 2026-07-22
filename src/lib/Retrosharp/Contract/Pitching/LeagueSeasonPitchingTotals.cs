namespace Retrosharp.Contract.Pitching
{
    /// <summary>
    /// League-wide pitching totals for one league-season, used solely to derive FIP's
    /// normalizing constant from Retrosharp's own imported data rather than a fixed external
    /// value. See spec/api.md, "FIP includes a league-normalizing constant, computed from
    /// Retrosharp's own data".
    /// </summary>
    public class LeagueSeasonPitchingTotals
    {
        public int LeagueId { get; set; }

        public short SeasonYear { get; set; }

        /// <summary>
        /// Summed from <c>GamePitchingStatistics.TeamEarnedRuns</c> across every franchise in
        /// this league-season -- the authoritative team-earned figure, not the sum of each
        /// pitcher's individually-earned runs.
        /// </summary>
        public int TeamEarnedRuns { get; set; }

        /// <summary>
        /// Summed from <see cref="Retrosharp.Contract.GameEvent.PitcherEventAggregate.HomerunsAllowed"/>
        /// across every pitcher in this league-season.
        /// </summary>
        public int HomerunsAllowed { get; set; }

        /// <summary>
        /// Summed from <c>Pitching.BaseOnBalls</c> across every pitcher in this league-season.
        /// </summary>
        public int BaseOnBalls { get; set; }

        /// <summary>
        /// Summed from <c>Pitching.HitBatsmen</c> across every pitcher in this league-season.
        /// </summary>
        public int HitBatsmen { get; set; }

        /// <summary>
        /// Summed from <c>Pitching.Strikeouts</c> across every pitcher in this league-season.
        /// </summary>
        public int Strikeouts { get; set; }

        /// <summary>
        /// Summed from <c>Pitching.InningsPitched</c> (stored as outs, not innings) across
        /// every pitcher in this league-season.
        /// </summary>
        public int InningsPitchedOuts { get; set; }
    }
}
