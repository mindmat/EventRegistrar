import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { AssignmentCandidate, SettlePaymentService } from './settle-payment.service';

@Component({
  selector: 'app-settle-payment',
  templateUrl: './settle-payment.component.html'
})
export class SettlePaymentComponent implements OnInit
{
  private unsubscribeAll: Subject<any> = new Subject<any>();
  candidates: AssignmentCandidate[];

  constructor(private service: SettlePaymentService, private changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void
  {
    this.service.candidates$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((candidates: AssignmentCandidate[]) =>
      {
        this.candidates = candidates;

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });

  }
  unassign(paymentAssignmentId: string)
  {
    this.service.unassign(paymentAssignmentId);
  }
}
