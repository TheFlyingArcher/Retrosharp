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
