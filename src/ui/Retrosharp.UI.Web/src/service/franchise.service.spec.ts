import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { FranchiseService } from './franchise.service';
import { HttpService } from './http.service';

describe('FranchiseService', () => {
  let service: FranchiseService;
  let getAsync: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    getAsync = vi.fn().mockReturnValue(of({ items: [], totalCount: 0, limit: 50, offset: 0 }));
    TestBed.configureTestingModule({
      providers: [{ provide: HttpService, useValue: { getAsync } }],
    });
    service = TestBed.inject(FranchiseService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('requests former names with paging params', () => {
    service.getFormerNamesAsync(50, 10).subscribe();

    expect(getAsync).toHaveBeenCalledWith(expect.stringMatching(/\/teams\/former-names\?limit=50&offset=10$/));
  });

  it('requests all-time summaries with paging params', () => {
    service.getAllTimeAsync(25, 0).subscribe();

    expect(getAsync).toHaveBeenCalledWith(expect.stringMatching(/\/teams\?limit=25&offset=0$/));
  });
});
