import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { AssignmentCandidateRegistration } from '../settle-payment.service';

@Component({
  selector: 'app-assignment-candidate-registration',
  templateUrl: './assignment-candidate-registration.component.html'
})
export class AssignmentCandidateRegistrationComponent implements OnInit, OnChanges
{
  public candidateForm: FormGroup;
  @Input() candidate?: AssignmentCandidateRegistration;

  constructor(private fb: FormBuilder) { }

  ngOnInit(): void
  {
    this.fb.group({});
  }

  ngOnChanges(changes: SimpleChanges): void
  {
    console.log(changes);

    // Active item id
    if ('candidate' in changes)
    {
      var amount = this.candidate.price - this.candidate.amountPaid;
      this.candidateForm = this.fb.group({
        amountAssigned: [amount, [Validators.required, Validators.min(0.01), Validators.max(amount)]],
      });
    }
  }
}
