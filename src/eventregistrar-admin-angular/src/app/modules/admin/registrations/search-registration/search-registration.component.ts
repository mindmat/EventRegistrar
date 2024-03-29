import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { RegistrationMatch, UnprocessedRawRegistrationsInfo } from 'app/api/api';
import { BehaviorSubject, combineLatest, debounceTime, Subject, takeUntil } from 'rxjs';
import { NavigatorService } from '../../navigator.service';
import { UnprocessedRawRegistrationsService } from '../unprocessed-raw-registrations.service';
import { SearchRegistrationService } from './search-registration.service';

@Component({
  selector: 'app-search-registration',
  templateUrl: './search-registration.component.html'
})
export class SearchRegistrationComponent implements OnInit
{
  matches: RegistrationMatch[];
  unprocessedRawRegistrationsInfo: UnprocessedRawRegistrationsInfo;

  filters: {
    // categoryTag$: BehaviorSubject<string>;
    searchString$: BehaviorSubject<string>;
    // hideCompleted$: BehaviorSubject<boolean>;
  } = {
      // categoryTag$: new BehaviorSubject('all'),
      searchString$: new BehaviorSubject(''),
      // hideCompleted$: new BehaviorSubject(false)
    };
  private unsubscribeAll: Subject<any> = new Subject<any>();

  constructor(private service: SearchRegistrationService,
    private unprocessedRawRegistrationsService: UnprocessedRawRegistrationsService,
    public navigator: NavigatorService,
    private changeDetectorRef: ChangeDetectorRef,
    private route: ActivatedRoute,
    private router: Router) { }

  ngOnInit(): void
  {
    // use search string from url
    this.route.queryParams.subscribe((params) =>
    {
      this.filters.searchString$.next(params.search ?? '');
    });


    this.service.list$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((matches: RegistrationMatch[]) =>
      {
        this.matches = matches;

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });

    // Filter
    combineLatest([this.filters.searchString$]).pipe(debounceTime(200))
      .subscribe(([searchString]) =>
      {
        searchString = searchString.toLowerCase();
        this.service.fetchItemsOf(searchString).subscribe();
      });

    this.unprocessedRawRegistrationsService.unprocessedRawRegistrationsInfo$
      .subscribe((info) =>
      {
        this.unprocessedRawRegistrationsInfo = info;
        this.changeDetectorRef.markForCheck();
      });
  }

  filterByQuery(searchString: string): void
  {
    // this.filters.searchString$.next(searchString);
    // put search string in url
    this.router.navigate(
      [],
      {
        relativeTo: this.route,
        queryParams: { search: searchString },
        queryParamsHandling: 'merge'
      });
  }

  startProcessAllPendingRawRegistrationsCommand(): void
  {
    this.unprocessedRawRegistrationsService.startProcessAllPendingRawRegistrationsCommand();
  }
}
