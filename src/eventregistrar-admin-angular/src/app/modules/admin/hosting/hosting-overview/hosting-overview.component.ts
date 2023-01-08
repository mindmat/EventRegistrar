import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { HostingOffer, HostingOffersAndRequests, HostingRequest } from 'app/api/api';
import { Subject, takeUntil } from 'rxjs';
import { NavigatorService } from '../../navigator.service';
import { HostingOverviewService } from './hosting-overview.service';

@Component({
  selector: 'app-hosting-overview',
  templateUrl: './hosting-overview.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HostingOverviewComponent implements OnInit
{
  private unsubscribeAll: Subject<any> = new Subject<any>();
  offers: HostingOffer[];
  requests: HostingRequest[];

  constructor(private changeDetectorRef: ChangeDetectorRef,
    private hostingService: HostingOverviewService,
    public navigator: NavigatorService) { }

  ngOnInit(): void
  {
    this.hostingService.hosting$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((hosting: HostingOffersAndRequests) =>
      {
        this.offers = hosting.offers;
        this.requests = hosting.requests;

        this.changeDetectorRef.markForCheck();
      });
  };
}
