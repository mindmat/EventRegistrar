import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ActivatedRoute, ActivatedRouteSnapshot, Router } from '@angular/router';
import { BehaviorSubject, Subject, takeUntil } from 'rxjs';
import { AssignmentCandidateRegistration, BookingAssignments, SettlePaymentService } from './settle-payment.service';

@Component({
  selector: 'app-settle-payment',
  templateUrl: './settle-payment.component.html'
})
export class SettlePaymentComponent implements OnInit
{
  private unsubscribeAll: Subject<any> = new Subject<any>();
  private bankAccountBookingId$: BehaviorSubject<string | null> = new BehaviorSubject<string | null>(null);
  candidates: BookingAssignments;

  constructor(private service: SettlePaymentService, private changeDetectorRef: ChangeDetectorRef, private route: ActivatedRoute) { }

  ngOnInit(): void
  {
    this.service.candidates$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((candidates: BookingAssignments) =>
      {
        candidates.registrationCandidates.forEach(candidate => candidate.amountToAssign = candidate.price - candidate.amountPaid);
        this.candidates = candidates;

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });

    this.route.queryParams.subscribe(params => 
    {
      this.bankAccountBookingId$.next(params.search ?? '');
    });
  }

  assign(registrationId: string)
  {
    this.service.assign(this.bankAccountBookingId$.value, registrationId, 0, false, '');
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
