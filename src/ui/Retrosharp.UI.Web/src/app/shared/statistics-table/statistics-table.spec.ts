import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { StatColumn } from './stat-column.model';
import { StatisticsTable } from './statistics-table';

interface Row {
  year: number;
  hits: number;
}

const COLUMNS: StatColumn<Row>[] = [
  { key: 'year', header: 'Year', value: (r) => r.year },
  { key: 'hits', header: 'H', value: (r) => r.hits },
];

describe('StatisticsTable', () => {
  let fixture: ComponentFixture<StatisticsTable<Row>>;
  let component: StatisticsTable<Row>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StatisticsTable],
    }).compileComponents();

    fixture = TestBed.createComponent(StatisticsTable<Row>);
    component = fixture.componentInstance;
  });

  function setInputs(rows: Row[], combinedTotal: Row | null = null): void {
    fixture.componentRef.setInput('columns', COLUMNS);
    fixture.componentRef.setInput('rows', rows);
    fixture.componentRef.setInput('combinedTotal', combinedTotal);
    fixture.detectChanges();
  }

  it('should create', () => {
    setInputs([]);
    expect(component).toBeTruthy();
  });

  it('shows the empty message when there are no rows and no combined total', () => {
    setInputs([]);
    const empty = fixture.debugElement.query(By.css('.statistics-table-empty'));
    expect(empty).toBeTruthy();
    expect(empty.nativeElement.textContent).toContain('No statistics available.');
  });

  it('renders one data row per input row', () => {
    setInputs([
      { year: 2021, hits: 100 },
      { year: 2022, hits: 150 },
    ]);
    const cells = fixture.debugElement.queryAll(By.css('td.mat-mdc-cell'));
    expect(cells.length).toBe(4);
    expect(cells[0].nativeElement.textContent.trim()).toBe('2021');
    expect(cells[3].nativeElement.textContent.trim()).toBe('150');
  });

  it('sorts rows when a sort change is emitted', () => {
    setInputs([
      { year: 2022, hits: 150 },
      { year: 2021, hits: 100 },
    ]);

    component.onSortChange({ active: 'year', direction: 'asc' });
    fixture.detectChanges();

    expect(component.sortedRows()[0].year).toBe(2021);
    expect(component.sortedRows()[1].year).toBe(2022);
  });

  it('applies defaultSort to row order and to the active sort-header arrow before any click', () => {
    fixture.componentRef.setInput('columns', COLUMNS);
    fixture.componentRef.setInput('rows', [
      { year: 2022, hits: 150 },
      { year: 2021, hits: 100 },
    ]);
    fixture.componentRef.setInput('combinedTotal', null);
    fixture.componentRef.setInput('defaultSort', { active: 'year', direction: 'asc' });
    fixture.detectChanges();

    expect(component.sortedRows()[0].year).toBe(2021);
    expect(component.sortedRows()[1].year).toBe(2022);

    const yearHeader = fixture.debugElement.query(By.css('th.mat-mdc-header-cell'));
    expect(yearHeader.nativeElement.getAttribute('aria-sort')).toBe('ascending');
  });

  it('lets an explicit sort change override defaultSort', () => {
    fixture.componentRef.setInput('columns', COLUMNS);
    fixture.componentRef.setInput('rows', [
      { year: 2022, hits: 150 },
      { year: 2021, hits: 100 },
    ]);
    fixture.componentRef.setInput('combinedTotal', null);
    fixture.componentRef.setInput('defaultSort', { active: 'year', direction: 'asc' });
    fixture.detectChanges();

    component.onSortChange({ active: 'year', direction: 'desc' });
    fixture.detectChanges();

    expect(component.sortedRows()[0].year).toBe(2022);
    expect(component.sortedRows()[1].year).toBe(2021);
  });

  it('renders a cell tooltip when the column defines one, and no title otherwise', () => {
    // Reuses the shared `fixture`/`Row` from beforeEach (rather than creating a second component
    // instance) -- this suite's zoneless change detection otherwise also flushes any other
    // still-uninitialized fixture created in the same test, and the `beforeEach`-created one
    // never gets its required inputs set except via `setInputs()`.
    const columns: StatColumn<Row>[] = [
      { key: 'year', header: 'Year', value: (r) => r.year, cellTooltip: (r) => `Year ${r.year}` },
      { key: 'hits', header: 'H', value: (r) => r.hits },
    ];
    fixture.componentRef.setInput('columns', columns);
    fixture.componentRef.setInput('rows', [{ year: 2021, hits: 100 }]);
    fixture.componentRef.setInput('combinedTotal', null);
    fixture.detectChanges();

    const cells = fixture.debugElement.queryAll(By.css('td.mat-mdc-cell'));
    expect(cells[0].nativeElement.getAttribute('title')).toBe('Year 2021');
    expect(cells[1].nativeElement.getAttribute('title')).toBe('');
  });

  it('pins the combined total as a footer row regardless of sort', () => {
    setInputs(
      [
        { year: 2022, hits: 150 },
        { year: 2021, hits: 100 },
      ],
      { year: 0, hits: 250 },
    );

    const footerCells = fixture.debugElement.queryAll(By.css('td.mat-mdc-footer-cell'));
    expect(footerCells.length).toBe(2);
    expect(footerCells[0].nativeElement.textContent.trim()).toBe('Total');
    expect(footerCells[1].nativeElement.textContent.trim()).toBe('250');
  });
});
