import { TestBed } from '@angular/core/testing';

import { BattingService } from './batting.service';

describe('BattingService', () => {
  let service: BattingService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(BattingService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
