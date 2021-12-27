import { TestBed } from '@angular/core/testing';

import { ParticipantsDoubleService } from './participants-double.service';

describe('ParticipantsDoubleService', () => {
  let service: ParticipantsDoubleService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ParticipantsDoubleService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
