import { AfterViewInit, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ActivatedRoute, ActivatedRouteSnapshot, Router } from '@angular/router';
import { BehaviorSubject, combineLatest, combineLatestWith, filter, fromEvent, merge, Observable, Subject, takeUntil, tap } from 'rxjs';
import { AssignmentRequest } from './assignment-candidate-registration/assignment-candidate-registration.component';
import { AssignmentCandidateRegistration, BookingAssignments, SettlePaymentService } from './settle-payment.service';

@Component({
  selector: 'app-settle-payment',
  templateUrl: './settle-payment.component.html'
})
export class SettlePaymentComponent implements OnInit
{
  private unsubscribeAll: Subject<any> = new Subject<any>();
  private bankAccountBookingId$: BehaviorSubject<string | null> = new BehaviorSubject<string | null>(null);
  private assignmentRequests: BehaviorSubject<AssignmentRequest | null> = new BehaviorSubject(null);

  candidates: BookingAssignments;

  constructor(private service: SettlePaymentService, private changeDetectorRef: ChangeDetectorRef, private route: ActivatedRoute) { }

  ngOnInit(): void
  {
    this.service.candidates$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((candidates: BookingAssignments) =>
      {
        candidates.registrationCandidates?.forEach(candidate => candidate.amountToAssign = candidate.price - candidate.amountPaid);
        this.candidates = candidates;

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });

    this.route.queryParams.subscribe(params => 
    {
      this.bankAccountBookingId$.next(params.search ?? '');
    });

    this.assignmentRequests.asObservable()
      .pipe(
        takeUntil(this.unsubscribeAll),
        filter(request => !!request))
      .subscribe((request) => this.service.assign(
        request.bankAccountBookingId,
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

  amountChanged(candidate: AssignmentCandidateRegistration, amountToAssign: number): void
  {
    candidate.amountToAssign = amountToAssign;
    candidate.difference = candidate.price - candidate.amountPaid - candidate.amountToAssign;
    console.log(candidate.difference);
    console.log(amountToAssign);
  }

}
