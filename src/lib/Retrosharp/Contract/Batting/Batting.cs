using System;

namespace Retrosharp.Contract.Batting
{
    /// <summary>
    /// Represents season batting statistics for a player on a franchise.
    /// </summary>
    public class Batting : Entity
    {
        /// <summary>
        /// Foreign key to the person (player).
        /// </summary>
        public int PersonId { get; set; }

        /// <summary>
        /// Foreign key to the franchise.
        /// </summary>
        public int FranchiseId { get; set; }

        /// <summary>
        /// Season year for these statistics.
        /// </summary>
        public short? SeasonYear { get; set; }

        /// <summary>
        /// Total plate appearances.
        /// </summary>
        public short PlateAppearances { get; set; }

        /// <summary>
        /// Total at-bats.
        /// </summary>
        public short AtBats { get; set; }

        /// <summary>
        /// Total hits.
        /// </summary>
        public short Hit { get; set; }

        /// <summary>
        /// Total doubles.
        /// </summary>
        public short Doubles { get; set; }

        /// <summary>
        /// Total triples.
        /// </summary>
        public short Triples { get; set; }

        /// <summary>
        /// Total home runs.
        /// </summary>
        public short Homeruns { get; set; }

        /// <summary>
        /// Total bases on balls (walks).
        /// </summary>
        public short BaseOnBalls { get; set; }

        /// <summary>
        /// Total strikeouts.
        /// </summary>
        public short Strikeouts { get; set; }

        /// <summary>
        /// Total sacrifice flies.
        /// </summary>
        public short SacrificeFlies { get; set; }

        /// <summary>
        /// Total sacrifice bunts.
        /// </summary>
        public short SacrificeBunts { get; set; }

        /// <summary>
        /// Total intentional bases on balls.
        /// </summary>
        public short IntentionalBb { get; set; }

        /// <summary>
        /// Total hit-by-pitches.
        /// </summary>
        public short HitByPitches { get; set; }

        /// <summary>
        /// Total stolen bases.
        /// </summary>
        public short StolenBases { get; set; }

        /// <summary>
        /// Total times caught stealing.
        /// </summary>
        public short TimesCaughtStealing { get; set; }

        /// <summary>
        /// Total runs scored.
        /// </summary>
        public short Runs { get; set; }

        /// <summary>
        /// Number of positions played.
        /// </summary>
        public short Positions { get; set; }

        /// <summary>
        /// Total grounded into double plays.
        /// </summary>
        public short GroundedIntoDoublePlay { get; set; }
    }
}
