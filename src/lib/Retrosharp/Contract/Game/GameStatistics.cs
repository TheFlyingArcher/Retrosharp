using System;

namespace Retrosharp.Contract.Game
{
    /// <summary>
    /// Represents aggregate offensive statistics for a team in a game.
    /// </summary>
    public class GameStatistics : Entity
    {
        /// <summary>
        /// Foreign key to the game.
        /// </summary>
        public int GameId { get; set; }

        /// <summary>
        /// Foreign key to the franchise.
        /// </summary>
        public int FranchiseId { get; set; }

        /// <summary>
        /// Indicates whether stats are for home or visitor team ("H" or "V").
        /// </summary>
        public string HomeVisitor { get; set; }

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
        /// Total runs batted in (RBIs).
        /// </summary>
        public short RunsBattedIn { get; set; }

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
        /// Total grounded into double plays.
        /// </summary>
        public short GroundedIntoDoublePlay { get; set; }
    }
}
