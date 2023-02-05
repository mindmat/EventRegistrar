import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { BehaviorSubject, Subject, combineLatest, takeUntil } from 'rxjs';
import { PaymentDifferencesService } from './payment-differences.service';
import { DifferencesDisplayItem } from 'app/api/api';
import { NavigatorService } from '../../navigator.service';

@Component({
  selector: 'app-payment-differences',
  templateUrl: './payment-differences.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PaymentDifferencesComponent implements OnInit
{
  filteredDifferences: DifferencesDisplayItem[];
  query$: BehaviorSubject<string> = new BehaviorSubject('');

  private _unsubscribeAll: Subject<any> = new Subject<any>();

  constructor(private _changeDetectorRef: ChangeDetectorRef,
    private paymentDifferencesService: PaymentDifferencesService,
    public navigator: NavigatorService) { }

  ngOnInit(): void
  {
    // Filter the courses
    combineLatest([this.query$, this.paymentDifferencesService.differences$])
      .pipe(takeUntil(this._unsubscribeAll))
      .subscribe(([query, differences]) =>
      {

        // Reset the filtered courses
        this.filteredDifferences = differences;

        // Filter by search query
        if (query !== '')
        {
          this.filteredDifferences = this.filteredDifferences.filter(
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
    this.query$.next(query);
  }

  refundDifference(registrationId: string)
  {
    this.paymentDifferencesService.refundDifference(registrationId);
  }

  trackByFn(index: number, item: any): any
  {
    return item.id || index;
  }
}
