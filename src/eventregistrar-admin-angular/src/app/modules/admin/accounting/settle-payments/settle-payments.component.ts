import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { MatSlideToggleChange } from '@angular/material/slide-toggle';
import { BehaviorSubject, combineLatest, debounceTime, Subject, takeUntil } from 'rxjs';
import { BankAccountBooking, BookingsOfDay, CreditDebit } from '../bankStatements/bankStatements.service';
import { BankBookingDisplayItem, SettlePaymentsService } from './settle-payments.service';

@Component({
  selector: 'app-settle-payments',
  templateUrl: './settle-payments.component.html'
})
export class SettlePaymentsComponent implements OnInit
{
  private unsubscribeAll: Subject<any> = new Subject<any>();
  daysWithBookings: BookingsOfDay[];
  CreditDebit = CreditDebit;
  selectedBooking: BankAccountBooking;

  filters: {
    query$: BehaviorSubject<string>;
    hideIncoming$: BehaviorSubject<boolean>;
    hideOutgoing$: BehaviorSubject<boolean>;
    hideSettled$: BehaviorSubject<boolean>;
    hideIgnored$: BehaviorSubject<boolean>;
  } = {
      query$: new BehaviorSubject(''),
      hideIncoming$: new BehaviorSubject(false),
      hideOutgoing$: new BehaviorSubject(false),
      hideSettled$: new BehaviorSubject(true),
      hideIgnored$: new BehaviorSubject(true),
    };

  constructor(private service: SettlePaymentsService, private changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void
  {
    this.service.payments$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((daysWithBookings: BookingsOfDay[]) =>
      {
        this.daysWithBookings = daysWithBookings;

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });

    // Filter
    combineLatest([this.filters.query$, this.filters.hideIncoming$, this.filters.hideOutgoing$, this.filters.hideSettled$, this.filters.hideIgnored$]).pipe(debounceTime(200))
      .subscribe(([query, hideIncoming, hideOutgoing, hideSettled, hideIgnored]) =>
      {
        query = query.toLowerCase();
        console.log('x');
        this.service.fetchBankStatements(hideIncoming, hideOutgoing, hideSettled, hideIgnored).subscribe(

        );
      });
  }

  selectBooking(booking: BankAccountBooking)
  {
    this.selectedBooking = booking;

    // Mark for check
    this.changeDetectorRef.markForCheck();
  }

  filterByQuery(query: string): void
  {
    this.filters.query$.next(query);
  }

  toggleIncoming(change: MatSlideToggleChange): void
  {
    this.filters.hideIncoming$.next(change.checked);
  }

  toggleOutgoing(change: MatSlideToggleChange): void
  {
    this.filters.hideOutgoing$.next(change.checked);
  }

  toggleSettled(change: MatSlideToggleChange): void
  {
    this.filters.hideSettled$.next(change.checked);
  }

  toggleIgnored(change: MatSlideToggleChange): void
  {
    this.filters.hideIgnored$.next(change.checked);
  }

}
