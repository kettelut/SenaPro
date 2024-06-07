import { TestBed } from '@angular/core/testing';

import { SenaProService } from './calculadora-cdb.service';

describe('SenaProService', () => {
  let service: SenaProService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SenaProService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
