import { AfterViewInit, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ActivatedRoute, ActivatedRouteSnapshot, Router } from '@angular/router';
import { AssignmentCandidateRegistration, ExistingAssignment, PaymentAssignments, PaymentType } from 'app/api/api';
import { BehaviorSubject, combineLatest, combineLatestWith, filter, fromEvent, merge, Observable, Subject, takeUntil, tap } from 'rxjs';
import { AssignmentRequest } from './assignment-candidate-registration/assignment-candidate-registration.component';
import { SettlePaymentService } from './settle-payment.service';

@Component({
  selector: 'app-settle-payment',
  templateUrl: './settle-payment.component.html'
})
export class SettlePaymentComponent implements OnInit
{
  private unsubscribeAll: Subject<any> = new Subject<any>();
  private paymentId$: BehaviorSubject<string | null> = new BehaviorSubject<string | null>(null);
  private assignmentRequests: BehaviorSubject<AssignmentRequest | null> = new BehaviorSubject(null);

  existingAssignments: ExistingAssignment[];
  candidates: AssignmentCandidateRegistrationEditItem[];
  openAmount?: number = null;
  paymentType?: PaymentType = null;

  PaymentType = PaymentType;

  constructor(private service: SettlePaymentService, private changeDetectorRef: ChangeDetectorRef, private route: ActivatedRoute) { }

  ngOnInit(): void
  {
    this.service.candidates$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((assignments: PaymentAssignments) =>
      {
        this.openAmount = assignments.openAmount;
        this.paymentType = assignments.type;
        this.existingAssignments = assignments.existingAssignments;
        this.candidates = assignments.registrationCandidates?.map(candidate => (
          {
            ...candidate,
            amountToAssign: candidate.price - candidate.amountPaid,
          } as AssignmentCandidateRegistrationEditItem));

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });

    this.route.queryParams.subscribe(params => 
    {
      this.paymentId$.next(params.search ?? '');
    });

    this.assignmentRequests.asObservable()
      .pipe(
        takeUntil(this.unsubscribeAll),
        filter(request => !!request))
      .subscribe((request) => this.service.assign(
        this.paymentType,
        request.paymentId,
        request.registrationId,
        request.amount,
        request.acceptDifference,
        request.acceptDifferenceReason));
  }

  assign(request: AssignmentRequest): void
  {
    console.log(request);
    this.assignmentRequests.next(request);
  }

  unassign(paymentAssignmentId: string)
  {
    this.service.unassign(paymentAssignmentId);
  }

  amountChanged(candidate: AssignmentCandidateRegistrationEditItem, amountToAssign: number): void
  {
    candidate.amountToAssign = amountToAssign;
    candidate.difference = candidate.price - candidate.amountPaid - candidate.amountToAssign;
  }
}


export interface AssignmentCandidateRegistrationEditItem extends AssignmentCandidateRegistration
{
  amountToAssign: number;
  difference: number;
  locked: boolean;
}