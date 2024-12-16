import { ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, Inject, OnInit, ViewChild } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { NavigatorService } from '../../navigator.service';
import { SpotMatchCandidate, SpotMatchCandidates, Role, Api } from 'app/api/api';
import { Subject, BehaviorSubject, takeUntil, debounce, interval, switchMap } from 'rxjs';
import { EventService } from '../../events/event.service';

@Component({
  selector: 'app-match-single-spots',
  templateUrl: './match-single-spots.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MatchSingleSpotsComponent implements OnInit
{
  private unsubscribeAll: Subject<any> = new Subject<any>();
  private searchQuery$: BehaviorSubject<string | null> = new BehaviorSubject<string | null>(null);
  result: SpotMatchCandidates;
  @ViewChild('query', { static: true }) searchElement: ElementRef | null;

  constructor(private changeDetectorRef: ChangeDetectorRef,
    @Inject(MAT_DIALOG_DATA) public data: { context: SpotMatchContext, spotId: string, partnerName: string; },
    public navigator: NavigatorService,
    private api: Api,
    private eventService: EventService) { }

  ngOnInit(): void
  {
    this.searchQuery$.pipe(
      takeUntil(this.unsubscribeAll),
      debounce(query =>
      {
        if (query != null)
        {
          return interval(300);
        }
        return interval(0);
      }),
      switchMap(searchString => this.api.spotMatchCandidates_Query({ eventId: this.eventService.selectedId, registrableId: this.data.context.registrableId, role: this.data.context.role, searchString }))
    )
      .subscribe((result: SpotMatchCandidates) =>
      {
        this.result = result;
        this.changeDetectorRef.markForCheck();
      });
  }

  assign(candidate: SpotMatchCandidate): void
  {
    let spotId_Leader: string;
    let spotId_Follower: string;
    if (this.data.context.role === Role.Leader)
    {
      spotId_Leader = candidate.spotId;
      spotId_Follower = this.data.spotId;
    }
    else
    {
      spotId_Leader = this.data.spotId;
      spotId_Follower = candidate.spotId;
    }

    this.api.matchSingleSpots_Command({ eventId: this.eventService.selectedId, spotId_Leader, spotId_Follower })
      .subscribe(x => console.log(x));
  }

  searchCandidates(query: string)
  {
    this.searchQuery$.next(query);
  }
}

class SpotMatchContext
{
  registrableId: string;
  role: Role;
}