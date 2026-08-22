/**
 * Defines one column of a `StatisticsTable`. `T` is the row type (e.g. `BattingLine`).
 */
export interface StatColumn<T> {
  /** Unique key for this column, used for the mat-table column def and sort-active matching. */
  key: string;

  /** Column header text, e.g. "Home Runs (HR)". */
  header: string;

  /** Optional header tooltip for a longer explanation than the header text allows. */
  tooltip?: string;

  /** Renders this column's cell value for a given row. */
  value: (row: T) => string | number;

  /** Value used for sorting, if different from the display `value` (e.g. sort by raw date). */
  sortValue?: (row: T) => string | number;
}
