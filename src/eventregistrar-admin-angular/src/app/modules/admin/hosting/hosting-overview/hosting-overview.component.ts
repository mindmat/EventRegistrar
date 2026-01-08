import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { HostingOffer, HostingRequest } from 'app/api/api';
import { BehaviorSubject, Subject, combineLatest, takeUntil } from 'rxjs';
import { NavigatorService } from '../../navigator.service';
import { HostingOverviewService } from './hosting-overview.service';

@Component({
  selector: 'app-hosting-overview',
  templateUrl: './hosting-overview.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    TranslateModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule
  ]
})
export class HostingOverviewComponent implements OnInit
{
  private unsubscribeAll: Subject<any> = new Subject<any>();
  filteredOffers: HostingOffer[];
  filteredRequests: HostingRequest[];
  query$: BehaviorSubject<string | null> = new BehaviorSubject(null);

  constructor(private changeDetectorRef: ChangeDetectorRef,
    private hostingService: HostingOverviewService,
    public navigator: NavigatorService)
  {
  }

  ngOnInit(): void
  {
    combineLatest([this.hostingService.hosting$, this.query$])
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe(([hosting, query]) =>
      {
        this.filteredOffers = hosting.offers;
        this.filteredRequests = hosting.requests;

        if (query != null && query !== '')
        {
          this.filteredOffers = hosting.offers.filter(reg => reg.displayName?.toLowerCase().includes(query.toLowerCase())
            || reg.email?.toLowerCase().includes(query.toLowerCase())
            || reg.phone?.toLowerCase().includes(query.toLowerCase()));
          this.filteredRequests = hosting.requests.filter(reg => reg.displayName?.toLowerCase().includes(query.toLowerCase())
            || reg.email?.toLowerCase().includes(query.toLowerCase())
            || reg.phone?.toLowerCase().includes(query.toLowerCase()));
        }

        this.changeDetectorRef.markForCheck();
      });
  }

  filterByQuery(query: string)
  {
    this.query$.next(query);
  }

  downloadHosting()
  {
    this.hostingService.downloadHostingXlsx();
  }
}
