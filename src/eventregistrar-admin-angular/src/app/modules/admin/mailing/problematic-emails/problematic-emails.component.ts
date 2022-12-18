import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { MailDeliverySeverity, ProblematicEmail, RegistrationMatch } from 'app/api/api';
import { BehaviorSubject, combineLatest, debounceTime, Subject, takeUntil } from 'rxjs';
import { NavigatorService } from '../../navigator.service';
import { ProblematicEmailsService } from './problematic-emails.service';

@Component({
  selector: 'app-problematic-emails',
  templateUrl: './problematic-emails.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProblematicEmailsComponent implements OnInit
{
  public searchString$: BehaviorSubject<string | null> = new BehaviorSubject<string | null>(null);
  private unsubscribeAll: Subject<any> = new Subject<any>();
  emails: ProblematicEmail[];
  emailsNoneSucceeded: ProblematicEmail[];
  emailsSomeSucceeded: ProblematicEmail[];
  emailsLastSucceeded: ProblematicEmail[];

  constructor(private service: ProblematicEmailsService,
    public navigator: NavigatorService,
    private changeDetectorRef: ChangeDetectorRef) { }

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
