import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule } from '@ngx-translate/core';
import { DifferencesDisplayItem } from 'app/api/api';
import { BehaviorSubject, Subject, combineLatest, takeUntil } from 'rxjs';
import { NavigatorService } from '../../navigator.service';
import { PaymentDifferencesService } from './payment-differences.service';
import { RouterModule } from '@angular/router';
import { UserHasRightDirective } from 'app/core/auth/user-has-right.directive';

@Component({
  selector: 'app-payment-differences',
  templateUrl: './payment-differences.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [
    CommonModule,
    TranslateModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatMenuModule,
    MatTooltipModule,
    RouterModule,
    UserHasRightDirective,
    MatProgressBarModule
  ]
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

  sendPleasePayDifferenceMail(registrationId: string)
  {
    this.paymentDifferencesService.sendPleasePayDifferenceMail(registrationId);
  }

  trackByFn(index: number, item: any): any
  {
    return item.id || index;
  }
}
