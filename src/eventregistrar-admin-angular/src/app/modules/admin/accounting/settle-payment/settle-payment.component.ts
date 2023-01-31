import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AssignmentCandidateRegistration, ExistingAssignment, PaymentAssignments, PaymentType, RegistrationMatch, RegistrationState, RepaymentCandidate } from 'app/api/api';
import { BehaviorSubject, debounceTime, filter, Subject, takeUntil } from 'rxjs';
import { NavigatorService } from '../../navigator.service';
import { AssignmentRequest, Payment, SettlementCandidate } from './assignment-candidate-registration/assignment-candidate-registration.component';
import { SearchRegistrationsForSettlementService } from './search-registrations-for-settlement.service';
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

  existingAssignments: ExistingAssignment[];
  candidates: AssignmentCandidateRegistrationEditItem[];
  repaymentCandidates: RepaymentCandidate[];
  searchMatches: (RegistrationMatch & { locked: boolean; amountMatch: boolean; })[];
  payment?: Payment | null = null;
  PaymentType = PaymentType;
  RegistrationState = RegistrationState;

  constructor(private service: SettlePaymentService,
    public navigator: NavigatorService,
    private changeDetectorRef: ChangeDetectorRef,
    private route: ActivatedRoute,
    private searchRegistrationsService: SearchRegistrationsForSettlementService) { }

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

        this.repaymentCandidates = assignments.repaymentCandidates?.map(candidate => (
          {
            ...candidate,
            amountToAssign: Math.min(this.payment.openAmount, candidate.amountUnsettled),
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
        this.payment.type,
        request.paymentId,
        request.registrationId,
        request.amount,
        request.acceptDifference,
        request.acceptDifferenceReason));

    this.searchQuery$.pipe(
      debounceTime(300),
    ).subscribe(query => this.searchRegistrationsService.fetchItemsOf(query));

    this.searchRegistrationsService.matches$.subscribe(matches =>
    {
      this.searchMatches = matches?.map(match =>
      {
        return {
          ...match,
          locked: false,
          amountMatch: this.payment.openAmount == (match.price - match.amountPaid)
        };
      });
      this.changeDetectorRef.markForCheck();
    });
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
    this.service.assignRepayment(this.payment.id, paymentId_Repayment, amountToAssign);
  }

  amountChanged(candidate: AssignmentCandidateRegistrationEditItem, amountToAssign: number): void
  {
    candidate.amountToAssign = amountToAssign;
    candidate.difference = candidate.price - candidate.amountPaid - candidate.amountToAssign;
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