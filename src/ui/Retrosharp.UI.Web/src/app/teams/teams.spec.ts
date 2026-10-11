import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { Teams } from './teams';
import { FranchiseService } from '../../service/franchise.service';

describe('Teams', () => {
  let fixture: ComponentFixture<Teams>;

  beforeEach(async () => {
    const empty = { items: [], totalCount: 0, limit: 50, offset: 0 };

    await TestBed.configureTestingModule({
      imports: [Teams],
      providers: [
        {
          provide: FranchiseService,
          useValue: { getAllTimeAsync: () => of(empty), getFormerNamesAsync: () => of(empty) },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Teams);
    await fixture.whenStable();
  });

  it('renders the current-franchises and former-names tables separately', () => {
    const el: HTMLElement = fixture.nativeElement;

    expect(el.querySelector('app-current-franchises h2')?.textContent).toBe('Current franchises');
    expect(el.querySelector('app-former-names h2')?.textContent).toBe('Former names');
  });
});
