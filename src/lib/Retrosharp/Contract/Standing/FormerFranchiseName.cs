namespace Retrosharp.Contract.Standing
{
    /// <summary>
    /// One earlier name of a franchise lineage -- a distinct (FranchiseLocation, Nickname) pair
    /// other than the lineage's current one -- with the span and record it covers. A name that
    /// recurs after a gap is a single row. See spec/former-franchise-names.md.
    /// </summary>
    public class FormerFranchiseName
    {
        /// <summary>
        /// This name's most recent era's Franchise.Id -- what <c>GET /teams/{id}</c> resolves.
        /// </summary>
        public int FranchiseId { get; set; }

        /// <summary>
        /// Franchise code of this name's most recent era (e.g. "MON").
        /// </summary>
        public string FranchiseCode { get; set; } = string.Empty;

        /// <summary>
        /// "FranchiseLocation Nickname", e.g. "Montreal Expos".
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Year of this name's first era's start.
        /// </summary>
        public short FirstSeasonYear { get; set; }

        /// <summary>
        /// Year of this name's last era's end. A recurring name may have gaps in between.
        /// </summary>
        public short LastSeasonYear { get; set; }

        /// <summary>
        /// The lineage's current (most recent) era's Franchise.Id.
        /// </summary>
        public int CurrentFranchiseId { get; set; }

        /// <summary>
        /// "FranchiseLocation Nickname" of the lineage's current era.
        /// </summary>
        public string CurrentName { get; set; } = string.Empty;

        public short GamesPlayed { get; set; }

        public short Wins { get; set; }

        public short Losses { get; set; }

        public short Ties { get; set; }

        /// <summary>
        /// Win percentage, ties excluded from the denominator. Not persisted.
        /// </summary>
        public float WinPercentage => (Wins + Losses) > 0 ? (float)Wins / (Wins + Losses) : 0f;
    }
}
