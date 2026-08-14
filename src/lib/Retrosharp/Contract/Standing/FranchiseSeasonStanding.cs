namespace Retrosharp.Contract.Standing
{
    /// <summary>
    /// One franchise's regular-season standing for one season -- precomputed from <c>Game</c>
    /// results rather than derived live on each request. See spec/api.md and
    /// spec/frontend-prototype.md's "Resolved: Standings Derivation" note.
    /// </summary>
    public class FranchiseSeasonStanding : Entity
    {
        /// <summary>
        /// Foreign key to the franchise (already the era-specific row for this season, since
        /// every <c>Game</c> resolves to the correct era of a franchise at import time).
        /// </summary>
        public int FranchiseId { get; set; }

        /// <summary>
        /// Season year these standings cover.
        /// </summary>
        public short SeasonYear { get; set; }

        /// <summary>
        /// Total wins.
        /// </summary>
        public short Wins { get; set; }

        /// <summary>
        /// Total losses.
        /// </summary>
        public short Losses { get; set; }

        /// <summary>
        /// Total ties (a completed game with equal runs -- rare, but real, in older eras before
        /// mandatory extra innings/lights).
        /// </summary>
        public short Ties { get; set; }

        /// <summary>
        /// Rank within this franchise's grouping for the season: division if
        /// <see cref="Retrosharp.Contract.Franchise.Franchise.DivisionCode"/> is populated for
        /// this franchise-season, otherwise league-wide. 1 is first place.
        /// </summary>
        public byte Rank { get; set; }

        /// <summary>
        /// Games behind the leader of this franchise's grouping (division or league, matching
        /// <see cref="Rank"/>). 0 for the leader itself.
        /// </summary>
        public decimal GamesBehind { get; set; }

        /// <summary>
        /// True if this franchise finished first within its division for the season. Only
        /// meaningful once divisional play exists (see <see cref="Rank"/>) -- always false for
        /// a franchise-season with no division.
        /// </summary>
        public bool DivisionChampion { get; set; }

        /// <summary>
        /// True if this franchise had the best regular-season record in its league for the
        /// season, independent of division. Pre-1969, this is exactly what "won the pennant"
        /// meant; post-1969, the real pennant went to the League Championship Series winner
        /// instead, which requires postseason data this project doesn't import -- see
        /// spec/frontend-prototype.md's "Resolved: Standings Derivation" note before using this
        /// as a "Pennant" column for a post-1969 season.
        /// </summary>
        public bool LeagueBestRecord { get; set; }

        /// <summary>
        /// Win percentage, ties excluded from the denominator (standard convention). Not
        /// persisted -- trivially derived from <see cref="Wins"/>/<see cref="Losses"/>, the same
        /// pattern <c>BattingStatistics</c> already uses for its own rate stats.
        /// </summary>
        public float WinPercentage => (Wins + Losses) > 0 ? (float)Wins / (Wins + Losses) : 0f;
    }
}
