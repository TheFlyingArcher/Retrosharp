export interface PagedResult<R> {
  /**
   * Total number of items in the result set. This is useful for pagination and knowing how many pages of data are available.
   */
  totalCount: number;

  /**
   * The maximum number of items to return in the result set.
   */
  limit: number;

  /**
   * The number of items to skip before starting to collect the result set.
   */
  offset: number;

  /**
   * The array of items in the result set.
   */
  items: R[];
}
