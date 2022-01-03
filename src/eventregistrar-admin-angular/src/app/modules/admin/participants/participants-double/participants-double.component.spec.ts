import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ParticipantsDoubleComponent } from './participants-double.component';

describe('ParticipantsDoubleComponent', () => {
  let component: ParticipantsDoubleComponent;
  let fixture: ComponentFixture<ParticipantsDoubleComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ParticipantsDoubleComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ParticipantsDoubleComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
