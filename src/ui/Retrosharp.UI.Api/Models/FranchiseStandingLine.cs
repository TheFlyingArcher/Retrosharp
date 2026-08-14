namespace Retrosharp.UI.Api.Models
{
    /// <summary>
    /// One franchise's precomputed regular-season standing for one season. See spec/api.md,
    /// "GET /teams/{id}/stats" and "GET /seasons/{year}/standings".
    /// </summary>
    public class FranchiseStandingLine
    {
        public short SeasonYear { get; set; }

        public short Wins { get; set; }

        public short Losses { get; set; }

        public short Ties { get; set; }

        public float WinPercentage { get; set; }

        /// <summary>
        /// Rank within division if this franchise-season has one, otherwise within league.
        /// </summary>
        public byte Rank { get; set; }

        public decimal GamesBehind { get; set; }

        /// <summary>
        /// True if <see cref="Rank"/> is division-scoped and this franchise finished first in it.
        /// </summary>
        public bool DivisionChampion { get; set; }

        /// <summary>
        /// True if this franchise had the best regular-season record in its league, independent
        /// of division. The pre-1969 "pennant" -- not a substitute for a post-1969 pennant,
        /// which went to the League Championship Series winner instead (postseason data this
        /// project doesn't import). See spec/frontend-prototype.md's "Resolved: Standings
        /// Derivation" note before displaying this as "Pennant" for a post-1969 season.
        /// </summary>
        public bool LeagueBestRecord { get; set; }
    }
}
