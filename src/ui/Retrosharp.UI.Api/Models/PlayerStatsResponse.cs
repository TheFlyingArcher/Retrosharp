namespace Retrosharp.UI.Api.Models
{
    /// <summary>
    /// Response envelope for a player's batting/pitching/fielding statistics, for one season or
    /// their whole career. See spec/api.md, "A season split across multiple franchises produces
    /// a combined total row".
    /// </summary>
    public class PlayerStatsResponse<T>
        where T : class
    {
        /// <summary>
        /// One row per franchise (and, for fielding, position) within the requested scope.
        /// </summary>
        public IEnumerable<T> Rows { get; set; } = Enumerable.Empty<T>();

        /// <summary>
        /// Every counting stat in <see cref="Rows"/> summed, with rate stats recomputed from
        /// the sums. Null when <see cref="Rows"/> has zero or one entries.
        /// </summary>
        public T? CombinedTotal { get; set; }
    }
}
