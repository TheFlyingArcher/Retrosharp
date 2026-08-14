namespace Retrosharp.Contract.Standing
{
    /// <summary>
    /// One franchise lineage's all-time record, spanning every era (name/city/division change)
    /// that lineage has had -- e.g. one row for "Washington Nationals" covering both its
    /// Montreal Expos and Washington Nationals eras, not two separate rows. See
    /// spec/frontend-prototype.md's "Resolved: Franchise All-Time Summary" note and
    /// spec/api.md, "GET /teams".
    /// </summary>
    public class FranchiseCareerSummary
    {
        /// <summary>
        /// The representative (most recent) era's Franchise.Id -- what "Name" links to on the
        /// Franchises page, and what <c>GET /teams/{franchiseId}</c> resolves.
        /// </summary>
        public int FranchiseId { get; set; }

        public string FranchiseCode { get; set; } = string.Empty;

        /// <summary>
        /// "PlayingCity Nickname" as of the most recent era.
        /// </summary>
        public string CurrentName { get; set; } = string.Empty;

        /// <summary>
        /// "PlayingCity Nickname" for every earlier era, oldest first. Empty if this lineage has
        /// only ever had one era.
        /// </summary>
        public IReadOnlyList<string> FormerNames { get; set; } = Array.Empty<string>();

        /// <summary>
        /// The year of this lineage's earliest era (its <c>FranchiseStart</c>), regardless of
        /// name/city changes since.
        /// </summary>
        public short FirstSeasonYear { get; set; }

        public short GamesPlayed { get; set; }

        public short Wins { get; set; }

        public short Losses { get; set; }

        public short Ties { get; set; }

        /// <summary>
        /// Number of seasons (across every era) this lineage finished with a winning record
        /// (win percentage strictly greater than .500).
        /// </summary>
        public int SeasonsAboveFiveHundred { get; set; }

        /// <summary>
        /// Number of seasons (across every era) this lineage finished with a losing record (win
        /// percentage strictly less than .500). A season at exactly .500 counts toward neither.
        /// </summary>
        public int SeasonsBelowFiveHundred { get; set; }

        /// <summary>
        /// All-time win percentage, ties excluded from the denominator (standard convention,
        /// matching <see cref="FranchiseSeasonStanding.WinPercentage"/>). Not persisted --
        /// trivially derived from <see cref="Wins"/>/<see cref="Losses"/>.
        /// </summary>
        public float WinPercentage => (Wins + Losses) > 0 ? (float)Wins / (Wins + Losses) : 0f;
    }
}
