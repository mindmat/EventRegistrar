import { ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, Inject, OnInit, ViewChild } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { NavigatorService } from '../../navigator.service';
import { SpotMatchCandidate, SpotMatchCandidates, Role, Api } from 'app/api/api';
import { Subject, BehaviorSubject, takeUntil, debounce, interval, switchMap } from 'rxjs';
import { EventService } from '../../events/event.service';
import { CreateEventComponent } from '../../events/select-event/create-event/create-event.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FuseCardModule } from '@fuse/components/card';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-match-single-spots',
  templateUrl: './match-single-spots.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [CommonModule, RouterModule, MatButtonModule, MatIconModule, MatFormFieldModule, MatInputModule, MatDialogModule, FuseCardModule, TranslateModule]
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
    private eventService: EventService,
    public matDialogRef: MatDialogRef<CreateEventComponent>) { }

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
      .subscribe(_ =>
      {
        // Close the dialog
        this.matDialogRef.close();
      });
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