import { TestBed } from '@angular/core/testing';

import { SpotsService } from './spots.service';

describe('SpotsService', () => {
  let service: SpotsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SpotsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
