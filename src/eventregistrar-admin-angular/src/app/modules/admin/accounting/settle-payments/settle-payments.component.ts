import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { MatCheckboxChange } from '@angular/material/checkbox';
import { PaymentDisplayItem2, BookingsOfDay, CreditDebit } from 'app/api/api';
import { BehaviorSubject, combineLatest, debounceTime, Subject, takeUntil } from 'rxjs';
import { SettlePaymentsService } from './settle-payments.service';
import { Router } from '@angular/router';
import { NavigatorService } from '../../navigator.service';

@Component({
  selector: 'app-settle-payments',
  templateUrl: './settle-payments.component.html'
})
export class SettlePaymentsComponent implements OnInit
{
  private unsubscribeAll: Subject<any> = new Subject<any>();
  daysWithBookings: BookingsOfDay[];
  CreditDebit = CreditDebit;
  selectedBooking: PaymentDisplayItem2;

  filters: {
    query$: BehaviorSubject<string>;
    hideIncoming$: BehaviorSubject<boolean>;
    hideOutgoing$: BehaviorSubject<boolean>;
    hideSettled$: BehaviorSubject<boolean>;
    hideIgnored$: BehaviorSubject<boolean>;
  } = {
      query$: new BehaviorSubject(''),
      hideIncoming$: new BehaviorSubject(false),
      hideOutgoing$: new BehaviorSubject(true),
      hideSettled$: new BehaviorSubject(true),
      hideIgnored$: new BehaviorSubject(true),
    };

  constructor(private service: SettlePaymentsService, private navigator: NavigatorService, private changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void
  {
    this.service.payments$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((daysWithBookings: BookingsOfDay[]) =>
      {
        this.daysWithBookings = daysWithBookings;
        if (!this.selectedBooking && daysWithBookings.length > 0 && daysWithBookings[0].bookings.length > 0)        
        {
          this.selectBooking(daysWithBookings[0].bookings[0]);
        }
        else if (!!this.selectedBooking)
        {
          let selectedItemInNewList = daysWithBookings.flatMap(d => d.bookings).find(b => b.id === this.selectedBooking.id);
          if (!selectedItemInNewList)
          {
            // previously selected item is not in the current list anymore -> select another
            this.selectBooking(daysWithBookings[0].bookings[0]);
          }
          else
          {
            // select again - at the initial selection the list might not have been present yet
            this.selectBooking(selectedItemInNewList);
          }
        }

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });

    // Filter
    combineLatest([this.filters.query$, this.filters.hideIncoming$, this.filters.hideOutgoing$, this.filters.hideSettled$, this.filters.hideIgnored$]).pipe(debounceTime(200))
      .subscribe(([query, hideIncoming, hideOutgoing, hideSettled, hideIgnored]) =>
      {
        query = query.toLowerCase();
        this.service.fetchBankStatements(query, hideIncoming, hideOutgoing, hideSettled, hideIgnored).subscribe();
      });
  }

  selectBooking(booking: PaymentDisplayItem2)
  {
    this.selectedBooking = booking;
    this.navigator.goToSettlePaymentUrl(booking.id);

    // Mark for check
    this.changeDetectorRef.markForCheck();
  }

  filterByQuery(query: string): void
  {
    this.filters.query$.next(query);
  }

  toggleIncoming(change: MatCheckboxChange): void
  {
    this.filters.hideIncoming$.next(change.checked);
  }

  toggleOutgoing(change: MatCheckboxChange): void
  {
    this.filters.hideOutgoing$.next(change.checked);
  }

  toggleSettled(change: MatCheckboxChange): void
  {
    this.filters.hideSettled$.next(change.checked);
  }

  toggleIgnored(change: MatCheckboxChange): void
  {
    this.filters.hideIgnored$.next(change.checked);
  }
}
