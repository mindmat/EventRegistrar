import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { PayoutDisplayItem, PayoutState } from 'app/api/api';
import { BehaviorSubject, Subject, combineLatest, takeUntil } from 'rxjs';
import { NavigatorService } from '../../navigator.service';
import { PayoutsService } from './payouts.service';

@Component({
  selector: 'app-payouts',
  templateUrl: './payouts.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PayoutsComponent implements OnInit
{
  filteredPendingPayouts: PayoutDisplayItem[];
  filteredPaidPayouts: PayoutDisplayItem[];
  filter$: BehaviorSubject<string> = new BehaviorSubject('');

  private _unsubscribeAll: Subject<any> = new Subject<any>();

  constructor(private _changeDetectorRef: ChangeDetectorRef,
    private service: PayoutsService,
    public navigator: NavigatorService) { }

  ngOnInit(): void
  {
    // Filter the courses
    combineLatest([this.filter$, this.service.payouts$])
      .pipe(takeUntil(this._unsubscribeAll))
      .subscribe(([query, payouts]) =>
      {

        // Reset the filtered courses
        this.filteredPendingPayouts = payouts.filter(rpy => rpy.state !== PayoutState.Confirmed);
        this.filteredPaidPayouts = payouts.filter(rpy => rpy.state === PayoutState.Confirmed);

        // Filter by search query
        if (query !== '')
        {
          this.filteredPendingPayouts = this.filteredPendingPayouts.filter(
            cnc => cnc.firstName.toLowerCase().includes(query.toLowerCase())
              || cnc.lastName?.toLowerCase().includes(query.toLowerCase()));
          this.filteredPaidPayouts = this.filteredPaidPayouts.filter(
            cnc => cnc.firstName.toLowerCase().includes(query.toLowerCase())
              || cnc.lastName?.toLowerCase().includes(query.toLowerCase()));
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
    this.filter$.next(query);
  }

  trackByFn(index: number, item: any): any
  {
    return item.id || index;
  }
}
