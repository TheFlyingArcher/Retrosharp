using System;

namespace Retrosharp.Contract.Pitching
{
    /// <summary>
    /// Represents season pitching statistics for a player on a franchise.
    /// </summary>
    public class Pitching : Entity
    {
        /// <summary>
        /// Foreign key to the person (pitcher).
        /// </summary>
        public int PersonId { get; set; }

        /// <summary>
        /// Foreign key to the franchise.
        /// </summary>
        public int FranchiseId { get; set; }

        /// <summary>
        /// Season year for these statistics.
        /// </summary>
        public short SeasonYear { get; set; }

        /// <summary>
        /// Position indicator (e.g., "P" for pitcher).
        /// </summary>
        public string Position { get; set; }

        /// <summary>
        /// Total games pitched.
        /// </summary>
        public short GamesPitched { get; set; }

        /// <summary>
        /// Total games started.
        /// </summary>
        public short GamesStarted { get; set; }

        /// <summary>
        /// Total games finished.
        /// </summary>
        public short GamesFinished { get; set; }

        /// <summary>
        /// Total complete games.
        /// </summary>
        public short CompleteGames { get; set; }

        /// <summary>
        /// Total shutouts.
        /// </summary>
        public short Shutouts { get; set; }

        /// <summary>
        /// Total saves.
        /// </summary>
        public short Saves { get; set; }

        /// <summary>
        /// Total innings pitched.
        /// </summary>
        public short InningsPitched { get; set; }

        /// <summary>
        /// Total hits allowed.
        /// </summary>
        public short Hits { get; set; }

        /// <summary>
        /// Total runs allowed.
        /// </summary>
        public short Runs { get; set; }

        /// <summary>
        /// Total earned runs allowed.
        /// </summary>
        public short EarnedRuns { get; set; }

        /// <summary>
        /// Total bases on balls (walks) issued.
        /// </summary>
        public short BaseOnBalls { get; set; }

        /// <summary>
        /// Total strikeouts recorded.
        /// </summary>
        public short Strikeouts { get; set; }

        /// <summary>
        /// Total intentional bases on balls issued.
        /// </summary>
        public short IntentionalBb { get; set; }

        /// <summary>
        /// Total batters hit by pitches.
        /// </summary>
        public short HitBatsmen { get; set; }

        /// <summary>
        /// Total balks.
        /// </summary>
        public short Balks { get; set; }

        /// <summary>
        /// Total wild pitches.
        /// </summary>
        public short WildPitches { get; set; }
    }
}
