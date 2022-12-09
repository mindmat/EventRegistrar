import { ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { PotentialPartnerMatch, PotentialPartners } from 'app/api/api';
import { BehaviorSubject, combineLatest, debounce, debounceTime, distinct, interval, of, Subject, takeUntil, tap } from 'rxjs';
import { NavigatorService } from '../../navigator.service';
import { MatchPartnerService } from './match-partner.service';

@Component({
  selector: 'app-match-partner',
  templateUrl: './match-partner.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MatchPartnerComponent implements OnInit
{
  private unsubscribeAll: Subject<any> = new Subject<any>();
  private registrationId$: BehaviorSubject<string | null> = new BehaviorSubject<string | null>(null);
  private searchQuery$: BehaviorSubject<string | null> = new BehaviorSubject<string | null>(null);
  candidates: PotentialPartners;
  @ViewChild('query', { static: true }) searchElement: ElementRef;

  constructor(private service: MatchPartnerService,
    public navigator: NavigatorService,
    private changeDetectorRef: ChangeDetectorRef,
    private route: ActivatedRoute) { }

  ngOnInit(): void
  {
    this.service.candidates$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((candidates: PotentialPartners) =>
      {
        if (this.registrationId$.value !== candidates?.registrationId)
        {
          // deprecate old request
          return;
        }

        if (this.candidates?.registrationId !== candidates?.registrationId)
        {
          this.searchElement.nativeElement.value = candidates?.declaredPartner;
        }

        this.candidates = candidates;

        this.changeDetectorRef.markForCheck();
      });

    this.route.params.subscribe(params => 
    {
      this.candidates = null;
      this.registrationId$.next(params.id);
      this.searchQuery$.next(null);
      this.changeDetectorRef.markForCheck();
    });

    var searchDebounced = this.searchQuery$.pipe(
      debounce(query =>
      {
        if (query != null)
        {
          return interval(300);
        }
        return interval(0);
      }),
    );

    combineLatest([this.registrationId$, searchDebounced])
      .subscribe(([registrationId, searchString]) => this.service.fetchCandidates(registrationId, searchString).subscribe());
  }

  assign(candidate: PotentialPartnerMatch): void
  {
    this.service.assign(this.registrationId$.value, candidate.registrationId);
  }

  transformToSingle()
  {
    this.service.transformToSingle(this.candidates.registrationId);
  }

  // unassign(paymentAssignmentId: string)
  // {
  //   this.service.unassign(paymentAssignmentId);
  // }

  searchCandidates(query: string)
  {
    this.searchQuery$.next(query);
  }
}