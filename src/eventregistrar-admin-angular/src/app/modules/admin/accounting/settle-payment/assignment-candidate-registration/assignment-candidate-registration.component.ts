import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AssignmentCandidateRegistration } from '../settle-payment.service';

@Component({
  selector: 'app-assignment-candidate-registration',
  templateUrl: './assignment-candidate-registration.component.html'
})
export class AssignmentCandidateRegistrationComponent implements OnInit, OnChanges
{
  public candidateForm: FormGroup;
  @Input() candidate?: AssignmentCandidateRegistration;
  @Output() assign = new EventEmitter<AssignmentRequest>();

  constructor(private fb: FormBuilder) { }

  ngOnInit(): void
  {

  }

  ngOnChanges(changes: SimpleChanges): void
  {
    // Active item id
    if ('candidate' in changes)
    {
      var amount = this.candidate.price - this.candidate.amountPaid;
      this.candidateForm = this.fb.group({
        amountAssigned: [amount, [Validators.required, Validators.min(0.01), Validators.max(amount)]],
        acceptDifference: [false, Validators.required],
        acceptDifferenceReason: ['']
      });
    }
  }

  public emitAssign(): void
  {
    if (!this.candidate.locked)
    {
      this.assign.emit({
        bankAccountBookingId: this.candidate.bankAccountBookingId,
        registrationId: this.candidate.registrationId,
        amount: this.candidateForm.get('amountAssigned').value,
        acceptDifference: this.candidateForm.get('acceptDifference').value,
        acceptDifferenceReason: this.candidateForm.get('acceptDifferenceReason').value,
      } as AssignmentRequest);
    }
  }
}

export interface AssignmentRequest
{
  bankAccountBookingId: string;
  registrationId: string;
  amount: number;
  acceptDifference: boolean;
  acceptDifferenceReason: string;
}