import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { Participant } from 'app/api/api';
import { BehaviorSubject, combineLatest, debounceTime, Subject, takeUntil } from 'rxjs';
import { NavigatorService } from '../../navigator.service';
import { AllParticipantsService } from './all-participants.service';

@Component({
  selector: 'app-all-participants',
  templateUrl: './all-participants.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    TranslateModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatInputModule
  ]
})
export class AllParticipantsComponent implements OnInit
{
  private unsubscribeAll: Subject<any> = new Subject<any>();
  matches: Participant[];
  searchString$: BehaviorSubject<string> = new BehaviorSubject('');
  includeWaitingList$: BehaviorSubject<boolean> = new BehaviorSubject(false);

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
    combineLatest([this.searchString$, this.includeWaitingList$]).pipe(debounceTime(200))
      .subscribe(([searchString, includeWaitingList]) =>
      {
        searchString = searchString.toLowerCase();
        this.service.fetchItemsOf(searchString, includeWaitingList).subscribe();
      });
  }

  download()
  {
    this.service.downloadXlsx(this.includeWaitingList$.value);
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

  toggleIncludeWaitingList(checked: boolean)
  {
    this.includeWaitingList$.next(checked);
  }
}
