import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { PageEvent } from '@angular/material/paginator';
import { buildPageWindow, ELLIPSIS, PageNumberPaginator } from './page-number-paginator';

describe('buildPageWindow', () => {
  it('shows every page when the total is small', () => {
    expect(buildPageWindow(1, 5)).toEqual([1, 2, 3, 4, 5]);
  });

  it('windows around the current page with ellipses for large totals', () => {
    expect(buildPageWindow(60, 110)).toEqual([1, ELLIPSIS, 58, 59, 60, 61, 62, ELLIPSIS, 110]);
  });

  it('omits the leading ellipsis when the window already reaches page 1', () => {
    expect(buildPageWindow(2, 110)).toEqual([1, 2, 3, 4, ELLIPSIS, 110]);
  });

  it('omits the trailing ellipsis when the window already reaches the last page', () => {
    expect(buildPageWindow(109, 110)).toEqual([1, ELLIPSIS, 107, 108, 109, 110]);
  });
});

describe('PageNumberPaginator', () => {
  let fixture: ComponentFixture<PageNumberPaginator>;
  let component: PageNumberPaginator;
  let emitted: PageEvent[];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PageNumberPaginator],
    }).compileComponents();

    fixture = TestBed.createComponent(PageNumberPaginator);
    component = fixture.componentInstance;
    emitted = [];
    component.page.subscribe((event) => emitted.push(event));
  });

  function setInputs(length: number, pageIndex: number, pageSize = 25): void {
    fixture.componentRef.setInput('length', length);
    fixture.componentRef.setInput('pageIndex', pageIndex);
    fixture.componentRef.setInput('pageSize', pageSize);
    fixture.detectChanges();
  }

  it('computes total pages and the 1-based current page', () => {
    setInputs(2751, 2, 25);
    expect(component.totalPages()).toBe(111);
    expect(component.currentPage()).toBe(3);
  });

  it('emits a PageEvent when a page number is clicked', () => {
    setInputs(2751, 0, 25);

    const buttons = fixture.debugElement.queryAll(By.css('button'));
    const pageTwoButton = buttons.find((b) => b.nativeElement.textContent.trim() === '2');
    pageTwoButton!.nativeElement.click();

    expect(emitted.length).toBe(1);
    expect(emitted[0]).toMatchObject({ pageIndex: 1, pageSize: 25, previousPageIndex: 0 });
  });

  it('does not emit when clicking the already-current page', () => {
    setInputs(2751, 2, 25);

    component.goToPage(3);
    expect(emitted.length).toBe(0);
  });

  it('clamps goToPage to the valid page range', () => {
    setInputs(100, 0, 25); // totalPages = 4, currentPage = 1
    component.goToPage(999);
    expect(emitted[0].pageIndex).toBe(3);

    setInputs(100, 3, 25); // currentPage = 4
    component.goToPage(-5);
    expect(emitted[1].pageIndex).toBe(0);
  });

  it('disables First/Previous on the first page and Next/Last on the last page', () => {
    setInputs(50, 0, 25);
    let buttons = fixture.debugElement.queryAll(By.css('button'));
    expect(buttons[0].nativeElement.disabled).toBe(true); // First
    expect(buttons[1].nativeElement.disabled).toBe(true); // Previous

    setInputs(50, 1, 25);
    buttons = fixture.debugElement.queryAll(By.css('button'));
    const last = buttons[buttons.length - 1];
    const next = buttons[buttons.length - 2];
    expect(next.nativeElement.disabled).toBe(true);
    expect(last.nativeElement.disabled).toBe(true);
  });

  it('keeps the first item of the current page visible when the page size changes', () => {
    setInputs(500, 4, 25); // items 100-124

    component.onPageSizeChange(50); // item 100 now lands on page index 2 (items 100-149)
    expect(emitted[0]).toMatchObject({ pageIndex: 2, pageSize: 50 });
  });
});
