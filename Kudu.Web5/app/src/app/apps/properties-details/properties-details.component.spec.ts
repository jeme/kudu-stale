import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PropertiesDetailsComponent } from './properties-details.component';

describe('ConfigurationDetailsComponent', () => {
  let component: PropertiesDetailsComponent;
  let fixture: ComponentFixture<PropertiesDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PropertiesDetailsComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PropertiesDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
