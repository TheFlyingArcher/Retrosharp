namespace Retrosharp.Contract.GameEvent
{
    /// <summary>
    /// A single play's fields needed to derive a pitcher's per-franchise-season event
    /// aggregate (home runs/fly balls/at-bats/sacrifice flies allowed). Projected directly
    /// from <see cref="GameEvent"/> joined to its <c>Game</c>, since <see cref="GameEvent"/>
    /// itself carries neither the fielding franchise nor the season year. See spec/api.md,
    /// "PitcherEventAggregate".
    /// </summary>
    public class PitcherGameEventRecord
    {
        /// <summary>
        /// The pitcher's own franchise for this play, derived from <c>Game.HomeFranchiseId</c>/
        /// <c>VisitorFranchiseId</c> and <see cref="GameEvent.TeamAtBat"/> -- the fielding side
        /// is whichever team is not at bat.
        /// </summary>
        public int FranchiseId { get; set; }

        public short SeasonYear { get; set; }

        public GameEventType EventType { get; set; }

        public BattedBallType? BattedBallType { get; set; }

        public bool IsSacHit { get; set; }

        public bool IsSacFly { get; set; }
    }
}
