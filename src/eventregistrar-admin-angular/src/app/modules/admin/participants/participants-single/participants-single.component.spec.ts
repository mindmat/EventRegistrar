import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ParticipantsSingleComponent } from './participants-single.component';

describe('ParticipantsSingleComponent', () => {
  let component: ParticipantsSingleComponent;
  let fixture: ComponentFixture<ParticipantsSingleComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ParticipantsSingleComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ParticipantsSingleComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
