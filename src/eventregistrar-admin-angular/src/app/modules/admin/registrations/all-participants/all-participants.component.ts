import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Participant, RegistrationMatch, UnprocessedRawRegistrationsInfo } from 'app/api/api';
import { Subject, BehaviorSubject, takeUntil, combineLatest, debounceTime } from 'rxjs';
import { NavigatorService } from '../../navigator.service';
import { SearchRegistrationService } from '../search-registration/search-registration.service';
import { UnprocessedRawRegistrationsService } from '../unprocessed-raw-registrations.service';
import { AllParticipantsService } from './all-participants.service';

@Component({
  selector: 'app-all-participants',
  templateUrl: './all-participants.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AllParticipantsComponent implements OnInit
{
  private unsubscribeAll: Subject<any> = new Subject<any>();
  matches: Participant[];
  searchString$: BehaviorSubject<string> = new BehaviorSubject('');

  constructor(private service: AllParticipantsService,
    public navigator: NavigatorService,
    private changeDetectorRef: ChangeDetectorRef,
    private route: ActivatedRoute,
    private router: Router) { }

  ngOnInit(): void
  {
    // use search string from url
    this.route.queryParams.subscribe(params => 
    {
      this.searchString$.next(params.search ?? '');
    });


    this.service.list$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((matches: Participant[]) =>
      {
        this.matches = matches;

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });

    // Filter
    combineLatest([this.searchString$]).pipe(debounceTime(200))
      .subscribe(([searchString]) =>
      {
        searchString = searchString.toLowerCase();
        this.service.fetchItemsOf(searchString).subscribe();
      });
  }

  download()
  {
    this.service.downloadXlsx();
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
}
