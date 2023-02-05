import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CancellationDisplayItem } from 'app/api/api';
import { BehaviorSubject, Subject, combineLatest, takeUntil } from 'rxjs';
import { CancellationsService } from './cancellations.service';
import { NavigatorService } from '../../navigator.service';

@Component({
  selector: 'app-cancellations',
  templateUrl: './cancellations.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CancellationsComponent implements OnInit
{
  filteredCancellationsWithPayments: CancellationDisplayItem[];
  filteredCancellationsWithoutPayments: CancellationDisplayItem[];
  filter$: BehaviorSubject<string> = new BehaviorSubject('');

  private _unsubscribeAll: Subject<any> = new Subject<any>();

  constructor(private _changeDetectorRef: ChangeDetectorRef,
    private service: CancellationsService,
    public navigator: NavigatorService) { }

  ngOnInit(): void
  {
    // Filter the courses
    combineLatest([this.filter$, this.service.cancellations$])
      .pipe(takeUntil(this._unsubscribeAll))
      .subscribe(([query, cancellations]) =>
      {

        // Reset the filtered courses
        this.filteredCancellationsWithPayments = cancellations.filter(cnc => cnc.paid > 0);
        this.filteredCancellationsWithoutPayments = cancellations.filter(cnc => !(cnc.paid > 0));

        // Filter by search query
        if (query !== '')
        {
          this.filteredCancellationsWithPayments = this.filteredCancellationsWithPayments.filter(
            cnc => cnc.firstName.toLowerCase().includes(query.toLowerCase())
              || cnc.lastName?.toLowerCase().includes(query.toLowerCase()));
          this.filteredCancellationsWithoutPayments = this.filteredCancellationsWithoutPayments.filter(
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
