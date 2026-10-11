import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';

import { CurrentFranchises } from './current-franchises';
import { FranchiseCareerSummary } from '../../../model/franchise-career-summary.model';
import { FranchiseService } from '../../../service/franchise.service';

const NATIONALS: FranchiseCareerSummary = {
  franchiseId: 2,
  franchiseCode: 'WAS',
  currentName: 'Washington Nationals',
  formerNames: ['Montreal Expos'],
  firstSeasonYear: 1969,
  gamesPlayed: 8000,
  wins: 3900,
  losses: 4100,
  ties: 0,
  winPercentage: 0.4879,
  seasonsAboveFiveHundred: 20,
  seasonsBelowFiveHundred: 36,
};

describe('CurrentFranchises', () => {
  let component: CurrentFranchises;
  let fixture: ComponentFixture<CurrentFranchises>;
  let getAllTimeAsync: ReturnType<typeof vi.fn>;

  async function create(response: unknown): Promise<void> {
    getAllTimeAsync = vi.fn().mockReturnValue(response);

    await TestBed.configureTestingModule({
      imports: [CurrentFranchises],
      providers: [{ provide: FranchiseService, useValue: { getAllTimeAsync } }],
    }).compileComponents();

    fixture = TestBed.createComponent(CurrentFranchises);
    component = fixture.componentInstance;
    await fixture.whenStable();
  }

  it('loads every franchise in a single request', async () => {
    await create(of({ items: [NATIONALS], totalCount: 1, limit: 100, offset: 0 }));

    expect(getAllTimeAsync).toHaveBeenCalledTimes(1);
    expect(getAllTimeAsync).toHaveBeenCalledWith(100, 0);
    expect(component.franchises()).toEqual([NATIONALS]);
  });

  it('renders no paginator and no ties column', async () => {
    await create(of({ items: [NATIONALS], totalCount: 1, limit: 100, offset: 0 }));
    const el: HTMLElement = fixture.nativeElement;

    expect(el.querySelector('app-page-number-paginator')).toBeNull();
    expect(component.displayedColumns).not.toContain('ties');
    expect(Array.from(el.querySelectorAll('th')).map((th) => th.textContent?.trim())).toEqual([
      'Franchise',
      'Since',
      'G',
      'W',
      'L',
      'W-L%',
      'G > .500',
    ]);
  });

  it('shows signed games over .500', async () => {
    await create(of({ items: [], totalCount: 0, limit: 100, offset: 0 }));

    expect(component.gamesOverFiveHundred(NATIONALS)).toBe('-200');
    expect(component.gamesOverFiveHundred({ ...NATIONALS, wins: 4100, losses: 3900 })).toBe('+200');
    expect(component.gamesOverFiveHundred({ ...NATIONALS, gamesPlayed: 0 })).toBe('—');
  });

  it('formats win percentage without a leading zero', async () => {
    await create(of({ items: [], totalCount: 0, limit: 100, offset: 0 }));

    expect(component.winPercentage(NATIONALS)).toBe('.488');
    expect(component.winPercentage({ ...NATIONALS, winPercentage: 0.5 })).toBe('.500');
  });

  it('shows a dash instead of zeroes when no games are on record', async () => {
    await create(of({ items: [], totalCount: 0, limit: 100, offset: 0 }));
    const empty = { ...NATIONALS, gamesPlayed: 0, wins: 0, losses: 0, ties: 0, winPercentage: 0 };

    expect(component.stat(empty, empty.wins)).toBe('—');
    expect(component.winPercentage(empty)).toBe('—');
    expect(component.stat(NATIONALS, NATIONALS.wins)).toBe('3900');
  });

  it('shows an error message when loading fails', async () => {
    vi.spyOn(console, 'error').mockImplementation(() => {});
    await create(throwError(() => new Error('boom')));

    expect(component.error()).toBe('Unable to load franchises. Please try again later.');
    expect(component.franchises()).toEqual([]);
  });
});
