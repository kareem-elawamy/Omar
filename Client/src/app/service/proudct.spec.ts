import { TestBed } from '@angular/core/testing';

import { Proudct } from './proudct';

describe('Proudct', () => {
  let service: Proudct;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Proudct);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
