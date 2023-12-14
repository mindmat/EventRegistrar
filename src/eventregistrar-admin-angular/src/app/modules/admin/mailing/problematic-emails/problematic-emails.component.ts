import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { MailDeliverySeverity, ProblematicEmail, RegistrationMatch } from 'app/api/api';
import { BehaviorSubject, combineLatest, debounceTime, Subject, takeUntil } from 'rxjs';
import { NavigatorService } from '../../navigator.service';
import { ProblematicEmailsService } from './problematic-emails.service';
import { ChartType } from 'angular-google-charts';
import { MailDeliverySuccessService } from './mail-delivery-success.service';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-problematic-emails',
  templateUrl: './problematic-emails.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProblematicEmailsComponent implements OnInit
{
  public searchString$: BehaviorSubject<string | null> = new BehaviorSubject<string | null>(null);
  emails: ProblematicEmail[];
  emailsNoneSucceeded: ProblematicEmail[];
  emailsSomeSucceeded: ProblematicEmail[];
  emailsLastSucceeded: ProblematicEmail[];
  ChartType = ChartType;
  chartColumns = [
    { type: 'string', role: 'From' },
    { type: 'string', role: 'To' },
    { type: 'number', role: 'Weight' }
  ];
  mailDeliveryData;
  recentlySentMailCount: number;
  private unsubscribeAll: Subject<any> = new Subject<any>();

  constructor(private service: ProblematicEmailsService,
    private deliveryService: MailDeliverySuccessService,
    public navigator: NavigatorService,
    private changeDetectorRef: ChangeDetectorRef,
    private translateService: TranslateService) { }

  ngOnInit(): void
  {
    this.service.list$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((emails: ProblematicEmail[]) =>
      {
        this.emails = emails;
        this.emailsNoneSucceeded = emails.filter(ml => ml.severity === MailDeliverySeverity.NoneSucceeded);
        this.emailsSomeSucceeded = emails.filter(ml => ml.severity === MailDeliverySeverity.SomeSucceeded);
        this.emailsLastSucceeded = emails.filter(ml => ml.severity === MailDeliverySeverity.LastSucceeded);

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });

    // Filter
    combineLatest([this.searchString$]).pipe(debounceTime(200))
      .subscribe(([searchString]) =>
      {
        searchString = searchString?.toLowerCase();
        this.service.fetchItems(searchString).subscribe();
      });

    const sent = this.translateService.instant('Sent');
    const processed = this.translateService.instant('MailState_Processed');
    const bounce = this.translateService.instant('MailState_Bounce');
    const dropped = this.translateService.instant('MailState_Dropped');
    const delivered = this.translateService.instant('MailState_Delivered');
    const opened = this.translateService.instant('MailState_Open');
    this.deliveryService.stats$.pipe(debounceTime(2000))
      .subscribe((stats) =>
      {
        this.mailDeliveryData = [
          [sent, processed, stats.processed + stats.bounce + stats.dropped + stats.delivered + stats.opened],
          [processed, delivered, stats.delivered + stats.opened],
          [delivered, opened, stats.opened],
          [processed, bounce, stats.bounce],
          [processed, dropped, stats.dropped],
        ];
        this.recentlySentMailCount = stats.sent;
        this.changeDetectorRef.markForCheck();
      });
  }


  filterByQuery(searchString: string): void
  {
    this.searchString$.next(searchString);
    // // put search string in url
    // this.router.navigate(
    //   [],
    //   {
    //     relativeTo: this.route,
    //     queryParams: { search: searchString },
    //     queryParamsHandling: 'merge'
    //   });
  }
}
