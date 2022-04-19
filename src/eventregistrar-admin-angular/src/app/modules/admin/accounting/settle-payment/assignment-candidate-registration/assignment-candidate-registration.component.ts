import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { FormBuilder, FormControl, FormGroup } from '@angular/forms';
import { AssignmentCandidateRegistration } from '../settle-payment.service';

@Component({
  selector: 'app-assignment-candidate-registration',
  templateUrl: './assignment-candidate-registration.component.html'
})
export class AssignmentCandidateRegistrationComponent implements OnInit, OnChanges
{
  candidateForm: FormGroup;
  @Input() candidate?: AssignmentCandidateRegistration;

  constructor(private formBuilder: FormBuilder) { }

  ngOnInit(): void
  {
  }

  ngOnChanges(changes: SimpleChanges): void
  {
    console.log(changes);

    // Active item id
    if ('candidate' in changes)
    {
      // Mark if active
      this.candidateForm = this.formBuilder.group({
        communication: [true],
        security: [true],
        meetups: [false],
        comments: [false],
        mention: [true],
        follow: [true],
        inquiry: [true]
      });
    }
  }
}
