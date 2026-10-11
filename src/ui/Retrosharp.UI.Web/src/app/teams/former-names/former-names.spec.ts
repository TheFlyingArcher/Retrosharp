import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';

import { FormerNames } from './former-names';
import { FormerFranchiseName } from '../../../model/former-franchise-name.model';
import { PagedResult } from '../../../model/paged-result.model';
import { FranchiseService } from '../../../service/franchise.service';

const EXPOS: FormerFranchiseName = {
  franchiseId: 1,
  franchiseCode: 'MON',
  name: 'Montreal Expos',
  firstSeasonYear: 1969,
  lastSeasonYear: 2004,
  currentFranchiseId: 2,
  currentName: 'Washington Nationals',
  gamesPlayed: 5800,
  wins: 2755,
  losses: 3045,
  ties: 0,
  winPercentage: 0.475,
};

describe('FormerNames', () => {
  let component: FormerNames;
  let fixture: ComponentFixture<FormerNames>;
  let getFormerNamesAsync: ReturnType<typeof vi.fn>;

  async function create(response: unknown): Promise<void> {
    getFormerNamesAsync = vi.fn().mockReturnValue(response);

    await TestBed.configureTestingModule({
      imports: [FormerNames],
      providers: [{ provide: FranchiseService, useValue: { getFormerNamesAsync } }],
    }).compileComponents();

    fixture = TestBed.createComponent(FormerNames);
    component = fixture.componentInstance;
    await fixture.whenStable();
  }

  it('loads the first page at the default page size', async () => {
    const result: PagedResult<FormerFranchiseName> = { items: [EXPOS], totalCount: 1, limit: 50, offset: 0 };
    await create(of(result));

    expect(getFormerNamesAsync).toHaveBeenCalledWith(50, 0);
    expect(component.formerNames()).toEqual([EXPOS]);
    expect(component.totalCount()).toBe(1);
  });

  it('formats years and win percentage', async () => {
    await create(of({ items: [], totalCount: 0, limit: 50, offset: 0 }));

    expect(component.years(EXPOS)).toBe('1969–2004');
    expect(component.years({ ...EXPOS, firstSeasonYear: 1884, lastSeasonYear: 1884 })).toBe('1884');
    expect(component.winPercentage(EXPOS)).toBe('.475');
    expect(component.stat(EXPOS, EXPOS.wins)).toBe('2755');
  });

  it('shows signed games over .500', async () => {
    await create(of({ items: [], totalCount: 0, limit: 50, offset: 0 }));

    expect(component.gamesOverFiveHundred(EXPOS)).toBe('-290');
    expect(component.gamesOverFiveHundred({ ...EXPOS, wins: 3000, losses: 2800 })).toBe('+200');
    expect(component.gamesOverFiveHundred({ ...EXPOS, gamesPlayed: 0 })).toBe('—');
  });

  it('shows a dash instead of zeroes when no games are on record', async () => {
    await create(of({ items: [], totalCount: 0, limit: 50, offset: 0 }));
    const empty = { ...EXPOS, gamesPlayed: 0, wins: 0, losses: 0, ties: 0, winPercentage: 0 };

    expect(component.stat(empty, empty.wins)).toBe('—');
    expect(component.winPercentage(empty)).toBe('—');
  });

  it('reloads with the new offset on page change', async () => {
    await create(of({ items: [], totalCount: 48, limit: 25, offset: 0 }));

    component.onPage({ pageIndex: 1, pageSize: 25, length: 48 });

    expect(getFormerNamesAsync).toHaveBeenLastCalledWith(25, 25);
  });

  it('shows an error message when loading fails', async () => {
    vi.spyOn(console, 'error').mockImplementation(() => {});
    await create(throwError(() => new Error('boom')));

    expect(component.error()).toBe('Unable to load former names. Please try again later.');
    expect(component.formerNames()).toEqual([]);
  });
});
