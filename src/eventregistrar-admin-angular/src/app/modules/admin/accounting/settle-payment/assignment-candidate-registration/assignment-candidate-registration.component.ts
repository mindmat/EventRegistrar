import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { PaymentType } from 'app/api/api';
import { NavigatorService } from 'app/modules/admin/navigator.service';
import { AssignmentCandidateRegistrationEditItem } from '../settle-payment.component';

@Component({
  selector: 'app-assignment-candidate-registration',
  templateUrl: './assignment-candidate-registration.component.html'
})
export class AssignmentCandidateRegistrationComponent implements OnInit, OnChanges
{
  public candidateForm: FormGroup;
  public possibleAmount: number;
  @Input() candidate?: AssignmentCandidateRegistrationEditItem;
  @Input() paymentType?: PaymentType;
  @Output() assign = new EventEmitter<AssignmentRequest>();

  constructor(private fb: FormBuilder,
    public navigator: NavigatorService) { }

  ngOnInit(): void
  {

  }

  ngOnChanges(changes: SimpleChanges): void
  {
    // Active item id
    if ('candidate' in changes)
    {
      this.possibleAmount = this.paymentType == PaymentType.Incoming
        ? Math.max(0, this.candidate.price - this.candidate.amountPaid)
        : Math.max(0, this.candidate.amountPaid);
      this.candidateForm = this.fb.group({
        amountAssigned: [this.possibleAmount, [Validators.required, Validators.min(0.01), Validators.max(this.possibleAmount)]],
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
        paymentId: this.candidate.paymentId,
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
  paymentId: string;
  registrationId: string;
  amount: number;
  acceptDifference: boolean;
  acceptDifferenceReason: string;
}