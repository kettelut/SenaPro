import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SenaProComponent } from './calculadora-cdb.component';

describe('SenaProComponent', () => {
  let component: SenaProComponent;
  let fixture: ComponentFixture<SenaProComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [SenaProComponent]
    })
      .compileComponents();

    fixture = TestBed.createComponent(SenaProComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
