import { Component, computed, input, signal } from '@angular/core';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { StatColumn } from './stat-column.model';

/**
 * Shared, sortable statistics table used across the Player Detail, Franchise Detail, Franchise
 * Season Detail, and Season Detail pages (see spec/frontend-prototype.md, "Shared Components ->
 * Statistics Tables"). Column set and row shape are supplied by the caller via `columns`/`rows`,
 * so this component has no baseball-specific knowledge of its own.
 *
 * An optional `combinedTotal` row is pinned as a footer row rather than participating in sorting,
 * since it should always stay visible regardless of how the caller has sorted the other rows.
 */
@Component({
  selector: 'app-statistics-table',
  standalone: true,
  templateUrl: './statistics-table.html',
  styleUrl: './statistics-table.css',
  imports: [MatTableModule, MatSortModule],
})
export class StatisticsTable<T> {
  readonly columns = input.required<StatColumn<T>[]>();
  readonly rows = input.required<T[]>();
  readonly combinedTotal = input<T | null>(null);
  readonly combinedTotalLabel = input('Total');
  readonly emptyMessage = input('No statistics available.');

  readonly displayedColumns = computed(() => this.columns().map((c) => c.key));

  private readonly sortState = signal<Sort | null>(null);

  readonly sortedRows = computed(() => {
    const sort = this.sortState();
    const rows = this.rows();
    if (!sort || !sort.active || sort.direction === '') {
      return rows;
    }

    const column = this.columns().find((c) => c.key === sort.active);
    if (!column) {
      return rows;
    }

    const accessor = column.sortValue ?? column.value;
    const direction = sort.direction === 'asc' ? 1 : -1;
    return [...rows].sort((a, b) => {
      const aValue = accessor(a);
      const bValue = accessor(b);
      if (aValue === bValue) {
        return 0;
      }
      return (aValue < bValue ? -1 : 1) * direction;
    });
  });

  onSortChange(sort: Sort): void {
    this.sortState.set(sort);
  }
}
