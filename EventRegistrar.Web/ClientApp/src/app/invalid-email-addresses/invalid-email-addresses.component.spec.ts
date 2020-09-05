import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { InvalidEmailAddressesComponent } from './invalid-email-addresses.component';

describe('InvalidEmailAddressesComponent', () => {
  let component: InvalidEmailAddressesComponent;
  let fixture: ComponentFixture<InvalidEmailAddressesComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ InvalidEmailAddressesComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(InvalidEmailAddressesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
