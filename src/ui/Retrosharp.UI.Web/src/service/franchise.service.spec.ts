import { TestBed } from '@angular/core/testing';

import { FranchiseService } from './franchise.service';

describe('FranchiseService', () => {
  let service: FranchiseService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(FranchiseService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
