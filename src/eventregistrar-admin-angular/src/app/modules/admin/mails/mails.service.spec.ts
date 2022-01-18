import { TestBed } from '@angular/core/testing';

import { MailsService } from './mails.service';

describe('MailsService', () => {
  let service: MailsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MailsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
