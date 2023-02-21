import { ChangeDetectorRef, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AssignedPayoutRequest, AssignedRepayment, AssignmentCandidateRegistration, ExistingAssignment, PaymentAssignments, PaymentType, PayoutRequestCandidate, RegistrationState, RepaymentCandidate } from 'app/api/api';
import { BehaviorSubject, debounceTime, filter, Subject, takeUntil } from 'rxjs';
import { NavigatorService } from '../../navigator.service';
import { AssignmentRequest, Payment, SettlementCandidate } from './assignment-candidate-registration/assignment-candidate-registration.component';
import { SettlePaymentService } from './settle-payment.service';

@Component({
  selector: 'app-settle-payment',
  templateUrl: './settle-payment.component.html'
})
export class SettlePaymentComponent implements OnInit
{
  private unsubscribeAll: Subject<any> = new Subject<any>();
  private paymentId$: BehaviorSubject<string | null> = new BehaviorSubject<string | null>(null);
  private searchQuery$: BehaviorSubject<string | null> = new BehaviorSubject<string | null>(null);
  private assignmentRequests: BehaviorSubject<AssignmentRequest | null> = new BehaviorSubject(null);
  @ViewChild('query', { static: true }) searchElement: ElementRef;

  existingAssignments: ExistingAssignment[];
  candidates: AssignmentCandidateRegistrationEditItem[];
  assignedRepayments: AssignedRepayment[];
  repaymentCandidates: RepaymentCandidate[];
  assignedPayoutRequests: AssignedPayoutRequest[];
  payoutRequestCandidates: PayoutRequestCandidateEditItem[];

  payment?: Payment | null = null;

  PaymentType = PaymentType;
  RegistrationState = RegistrationState;

  constructor(private service: SettlePaymentService,
    public navigator: NavigatorService,
    private changeDetectorRef: ChangeDetectorRef,
    private route: ActivatedRoute) { }

  ngOnInit(): void
  {
    this.service.candidates$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((assignments: PaymentAssignments) =>
      {
        this.payment = {
          id: this.service.paymentId,
          openAmount: assignments.openAmount,
          type: assignments.type,
          ignored: assignments.ignored
        };
        this.existingAssignments = assignments.existingAssignments;
        this.candidates = assignments.registrationCandidates?.map(candidate => (
          {
            ...candidate,
            amountToAssign: candidate.price - candidate.amountPaid,
          } as AssignmentCandidateRegistrationEditItem));

        // repayments
        this.assignedRepayments = assignments.assignedRepayments;
        this.repaymentCandidates = assignments.repaymentCandidates?.map(candidate => (
          {
            ...candidate,
            amountToAssign: Math.min(this.payment.openAmount, candidate.amountUnsettled),
          } as AssignmentCandidateRegistrationEditItem));

        // payouts
        this.assignedPayoutRequests = assignments.assignedPayoutRequests;
        this.payoutRequestCandidates = assignments.payoutRequestCandidates?.map(candidate => (
          {
            ...candidate,
            amountToAssign: Math.min(this.payment.openAmount, candidate.amount),
          } as PayoutRequestCandidateEditItem));

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });

    this.route.params.subscribe(params => 
    {
      this.searchQuery$.next(null);
      this.searchElement.nativeElement.value = null;
      this.changeDetectorRef.markForCheck();
    });

    this.route.queryParams.subscribe(params => 
    {
      this.paymentId$.next(params.search ?? '');
    });

    this.assignmentRequests.asObservable()
      .pipe(
        takeUntil(this.unsubscribeAll),
        filter(request => !!request && request.amount > 0))
      .subscribe((request) => this.service.assign(
        this.payment.type,
        request.paymentId,
        request.registrationId,
        request.amount,
        request.acceptDifference,
        request.acceptDifferenceReason));

    this.searchQuery$.pipe(
      debounceTime(300),
      filter(_ => !!this.payment?.id),
    ).subscribe(query => this.service.fetchCandidates(this.payment.id, query).subscribe());
  }

  assign(request: AssignmentRequest): void
  {
    this.assignmentRequests.next(request);
  }

  unassign(paymentAssignmentId: string)
  {
    this.service.unassign(paymentAssignmentId);
  }

  assignRepayment(paymentId_Repayment: string, amountToAssign: number)
  {
    if (amountToAssign > 0)
    {
      this.service.assignRepayment(this.payment.id, paymentId_Repayment, amountToAssign);
    }
  }

  amountChanged(candidate: AssignmentCandidateRegistrationEditItem, amountToAssign: number): void
  {
    candidate.amountToAssign = amountToAssign;
    candidate.difference = candidate.price - candidate.amountPaid - candidate.amountToAssign;
  }

  assignPayoutRequest(payoutRequestId: string, amountToAssign: number)
  {
    if (amountToAssign > 0)
    {
      this.service.assignPayoutRequest(this.payment.id, payoutRequestId, amountToAssign);
    }
  }

  searchCandidates(query: string)
  {
    this.searchQuery$.next(query);
  }

  ignorePayment()
  {
    if (!!this.payment?.id)
    {
      this.service.ignorePayment(this.payment.id);
    }
  }
}


export interface AssignmentCandidateRegistrationEditItem extends AssignmentCandidateRegistration, SettlementCandidate
{
  firstName: string;
  lastName: string;
  amountToAssign: number;
  difference: number;
  amountMatch: boolean;
}

export interface PayoutRequestCandidateEditItem extends PayoutRequestCandidate, SettlementCandidate
{
  amountToAssign: number;
}