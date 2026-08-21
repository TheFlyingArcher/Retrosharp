/**
 * Response envelope for a player's batting/pitching/fielding statistics, for one season or
 * their whole career. Mirrors `PlayerStatsResponse<T>` in Retrosharp.UI.Api.
 */
export interface PlayerStatsResponse<T> {
  /** One row per franchise (and, for fielding, position) within the requested scope. */
  rows: T[];

  /**
   * Every counting stat in `rows` summed, with rate stats recomputed from the sums. Null when
   * `rows` has zero or one entries.
   */
  combinedTotal: T | null;
}
