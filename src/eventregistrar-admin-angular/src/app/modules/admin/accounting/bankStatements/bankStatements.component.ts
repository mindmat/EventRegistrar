import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { BankAccountBooking, BankStatementsService, BookingsOfDay, CreditDebit } from './bankStatements.service';

@Component({
  selector: 'app-bankStatements',
  templateUrl: './bankStatements.component.html'
})
export class BankStatementsComponent implements OnInit
{
  private unsubscribeAll: Subject<any> = new Subject<any>();
  bookingDays: BookingsOfDay[];
  CreditDebit = CreditDebit;

  constructor(private service: BankStatementsService, private changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void
  {
    this.service.payments$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((bookings: BookingsOfDay[]) =>
      {
        this.bookingDays = bookings;

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });
  }

}
