namespace Retrosharp.Contract
{
    /// <summary>
    /// A player's statistic rows for a requested scope (one season, or their whole career),
    /// plus a combined total when more than one row applies -- for example, a season split
    /// across two franchises after a trade. See spec/api.md, "A season split across multiple
    /// franchises produces a combined total row".
    /// </summary>
    public class PlayerStatLines<T>
        where T : class
    {
        /// <summary>
        /// One row per (franchise[, position]) the player recorded statistics for within the
        /// requested scope, each denormalized with its franchise's identity for display.
        /// </summary>
        public IEnumerable<PlayerStatLine<T>> Rows { get; set; } = Enumerable.Empty<PlayerStatLine<T>>();

        /// <summary>
        /// Every counting stat in <see cref="Rows"/> summed, with rate stats recomputed from
        /// the sums. Null when <see cref="Rows"/> has zero or one entries, since a single row
        /// is already the total. Carries no franchise identity, since a combined total may
        /// span more than one franchise.
        /// </summary>
        public T? CombinedTotal { get; set; }
    }

    /// <summary>
    /// One statistics row, denormalized with the franchise it was recorded for. <typeparamref name="T"/>
    /// already carries FranchiseId (and, for Fielding, Position) -- this only adds what it doesn't:
    /// the franchise's display identity.
    /// </summary>
    public class PlayerStatLine<T>
        where T : class
    {
        public required T Stats { get; set; }

        public string FranchiseCode { get; set; } = string.Empty;

        public string FranchiseName { get; set; } = string.Empty;
    }
}
