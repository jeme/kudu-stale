import { TestBed } from '@angular/core/testing';

import { CurrentApplicationService } from './current-application.service';

describe('CurrentApplicationService', () => {
  let service: CurrentApplicationService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CurrentApplicationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
