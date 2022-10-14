import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { DuePaymentItem } from 'app/api/api';
import { BehaviorSubject, combineLatest, Subject, takeUntil } from 'rxjs';
import { DuePaymentsService } from './due-payments.service';

@Component({
  selector: 'app-due-payments',
  templateUrl: './due-payments.component.html'
})
export class DuePaymentsComponent implements OnInit
{
  // duePayments: DuePaymentItem[];
  filteredDuePayments: DuePaymentItem[];
  filters: {
    query$: BehaviorSubject<string>;
  } = {
      query$: new BehaviorSubject('')
    };

  private _unsubscribeAll: Subject<any> = new Subject<any>();

  constructor(private _changeDetectorRef: ChangeDetectorRef,
    private duePaymentsService: DuePaymentsService) { }

  ngOnInit(): void
  {
    // this.duePaymentsService.duePayments$
    //   .pipe(takeUntil(this._unsubscribeAll))
    //   .subscribe((duePayments: DuePaymentItem[]) =>
    //   {
    //     this.duePayments = duePayments;

    //     // Mark for check
    //     this._changeDetectorRef.markForCheck();
    //   });


    // Filter the courses
    combineLatest([this.filters.query$, this.duePaymentsService.duePayments$])
      .pipe(takeUntil(this._unsubscribeAll))
      .subscribe(([query, duePayments]) =>
      {

        // Reset the filtered courses
        this.filteredDuePayments = duePayments;

        // Filter by search query
        if (query !== '')
        {
          this.filteredDuePayments = this.filteredDuePayments.filter(
            dpm => dpm.firstName.toLowerCase().includes(query.toLowerCase())
              || dpm.lastName?.toLowerCase().includes(query.toLowerCase()));
        }

        // Mark for check
        this._changeDetectorRef.markForCheck();
      });
  }

  ngOnDestroy(): void
  {
    // Unsubscribe from all subscriptions
    this._unsubscribeAll.next(null);
    this._unsubscribeAll.complete();
  }

  filterByQuery(query: string): void
  {
    this.filters.query$.next(query);
  }

  trackByFn(index: number, item: any): any
  {
    return item.id || index;
  }

  sendReminderMail(duePayment: DuePaymentItem)
  {

  }

  sendReminderSms(duePayment: DuePaymentItem)
  {

  }
}
