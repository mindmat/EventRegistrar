import { ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { PotentialPartnerMatch, PotentialPartners } from 'app/api/api';
import { BehaviorSubject, combineLatest, debounceTime, merge, Subject, takeUntil } from 'rxjs';
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
        var registrationChanged = this.candidates?.registrationId !== candidates?.registrationId;
        this.candidates = candidates;
        if (registrationChanged)
        {
          this.searchElement.nativeElement.value = candidates.declaredPartner;
        }

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });

    this.route.params.subscribe(params => 
    {
      this.registrationId$.next(params.id);
    });

    combineLatest([this.registrationId$, this.searchQuery$.pipe(debounceTime(300))])
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