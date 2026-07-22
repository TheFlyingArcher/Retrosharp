namespace Retrosharp.Contract.GameEvent
{
    /// <summary>
    /// A pitcher's per-franchise-season event-derived totals, needed for HR/9, HR/FB, FIP, and
    /// pitcher BABIP -- none of which are stored as a <c>Pitching</c> aggregate column, since
    /// they depend on batted-ball outcomes tracked only at the <see cref="GameEvent"/> level.
    /// See spec/api.md, "PitcherEventAggregate".
    /// </summary>
    public class PitcherEventAggregate
    {
        public int PersonId { get; set; }

        public int FranchiseId { get; set; }

        public short SeasonYear { get; set; }

        /// <summary>
        /// Home runs allowed (primary <see cref="GameEvent.EventType"/> only, matching how
        /// <c>Batting.Homeruns</c> itself is derived).
        /// </summary>
        public int HomerunsAllowed { get; set; }

        /// <summary>
        /// Fly balls allowed, regardless of outcome (caught, dropped, or hit for a home run) --
        /// see game-event.md's own note on why HR/FB needs every fly ball, not just outs.
        /// </summary>
        public int FlyBallsAllowed { get; set; }

        /// <summary>
        /// Batters faced that count as an official at-bat, using the same classification
        /// <see cref="Retrosharp.Format.PlayByPlay.GameStatisticsResolver"/> already uses for the batter side.
        /// </summary>
        public int AtBatsAgainst { get; set; }

        /// <summary>
        /// Sacrifice flies allowed.
        /// </summary>
        public int SacrificeFliesAgainst { get; set; }
    }
}
