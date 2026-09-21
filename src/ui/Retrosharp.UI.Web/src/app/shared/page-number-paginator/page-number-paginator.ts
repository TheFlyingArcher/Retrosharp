import { Component, computed, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { PageEvent } from '@angular/material/paginator';

export const ELLIPSIS = '…';
const WINDOW_DELTA = 2;

/**
 * Windows a 1-based page range down to: page 1, page `total`, and `current` +/- `delta`, with an
 * `ELLIPSIS` marker standing in for each gap. E.g. current=60, total=110, delta=2 produces
 * [1, '…', 58, 59, 60, 61, 62, '…', 110] rather than 110 individual page buttons.
 */
export function buildPageWindow(current: number, total: number, delta = WINDOW_DELTA): (number | typeof ELLIPSIS)[] {
  const pages: number[] = [];
  for (let page = 1; page <= total; page++) {
    if (page === 1 || page === total || (page >= current - delta && page <= current + delta)) {
      pages.push(page);
    }
  }

  const windowed: (number | typeof ELLIPSIS)[] = [];
  let previous: number | undefined;
  for (const page of pages) {
    if (previous !== undefined) {
      const gap = page - previous;
      if (gap === 2) {
        // Exactly one page is missing between these two -- showing it costs no more room than
        // an ellipsis would, and reads better than "…" standing in for a single page number.
        windowed.push(previous + 1);
      } else if (gap > 2) {
        windowed.push(ELLIPSIS);
      }
    }
    windowed.push(page);
    previous = page;
  }
  return windowed;
}

/**
 * Numbered page-link paginator. Angular Material's own `mat-paginator` only ever renders
 * first/previous/next/last controls -- there's no built-in numbered-page-link UI -- so a browse
 * list with 100+ pages (e.g. every "M" surname, ~110 pages at 25/page) has no way to jump
 * directly to a page. See spec/frontend-ux-improvements.md, "Players Page: Numbered Paginator".
 *
 * Emits the same `PageEvent` shape `mat-paginator` does, so callers with an existing
 * `(page)="onPage($event)"` handler don't need to change it.
 */
@Component({
  selector: 'app-page-number-paginator',
  standalone: true,
  templateUrl: './page-number-paginator.html',
  styleUrl: './page-number-paginator.css',
  imports: [MatButtonModule, MatFormFieldModule, MatInputModule, MatSelectModule],
})
export class PageNumberPaginator {
  readonly length = input.required<number>();
  readonly pageIndex = input.required<number>();
  readonly pageSize = input.required<number>();
  readonly pageSizeOptions = input<number[]>([25, 50, 100]);

  readonly page = output<PageEvent>();

  readonly ellipsis = ELLIPSIS;

  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.length() / this.pageSize())));

  /** 1-based for display; the `pageIndex` input/output stays 0-based to match `PageEvent`. */
  readonly currentPage = computed(() => this.pageIndex() + 1);

  readonly pageWindow = computed(() => buildPageWindow(this.currentPage(), this.totalPages()));

  goToPage(page: number): void {
    const clamped = Math.min(Math.max(Math.trunc(page), 1), this.totalPages());
    if (clamped === this.currentPage()) {
      return;
    }
    this.emit(clamped - 1, this.pageSize());
  }

  onPageSizeChange(pageSize: number): void {
    // Keep the current page's first item visible under the new page size -- the same convention
    // mat-paginator itself uses when the page size changes.
    const firstItemIndex = this.pageIndex() * this.pageSize();
    this.emit(Math.floor(firstItemIndex / pageSize), pageSize);
  }

  onJumpInput(value: string): void {
    const page = Number(value);
    if (Number.isInteger(page) && value.trim() !== '') {
      this.goToPage(page);
    }
  }

  private emit(pageIndex: number, pageSize: number): void {
    this.page.emit({
      pageIndex,
      pageSize,
      length: this.length(),
      previousPageIndex: this.pageIndex(),
    });
  }
}
