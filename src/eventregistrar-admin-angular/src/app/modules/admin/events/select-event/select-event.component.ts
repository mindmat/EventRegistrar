import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { AccessRequest, CreateEventCommand, EventOfUser, EventSearchResult, EventState } from 'app/api/api';
import { BehaviorSubject, combineLatest, Subject, takeUntil } from 'rxjs';
import { v4 as createUuid } from 'uuid';
import { CreateEventComponent } from './create-event/create-event.component';
import { CreateEventService } from './create-event/create-event.service';
import { EventsOfUserService } from './events-of-user.service';
import { SearchEventsService } from './search-events.service';

@Component({
  selector: 'app-select-event',
  templateUrl: './select-event.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    TranslateModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatDialogModule
  ]
})
export class SelectEventComponent implements OnInit
{
  events: EventOfUser[];
  filteredEvents: EventOfUser[];
  requests: AccessRequest[];
  filteredRequests: AccessRequest[];
  otherEvents: EventSearchResult[];
  filteredOtherEvents: EventSearchResult[];

  finishedEventsInList: boolean = false;
  showFinishedEvents$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  finishedOtherEventsInList: boolean = false;
  showOtherFinishedEvents$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);

  createNewEventForm: FormGroup;

  query$: BehaviorSubject<string | null> = new BehaviorSubject(null);

  private unsubscribeAll: Subject<any> = new Subject<any>();

  constructor(private eventsOfUserService: EventsOfUserService,
    private searchEventService: SearchEventsService,
    private changeDetectorRef: ChangeDetectorRef,
    private fb: FormBuilder,
    private createEventService: CreateEventService,
    private matDialog: MatDialog) { }

  ngOnInit(): void
  {
    this.createNewEventForm = this.fb.group<CreateEventCommand>({
      id: createUuid(),
      name: '',
      acronym: '',
    });

    combineLatest([this.eventsOfUserService.events$, this.query$, this.showFinishedEvents$])
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe(([events, query, showFinishedEvents]) =>
      {
        this.events = events.authorizedEvents;
        this.filteredEvents = events.authorizedEvents;
        this.requests = events.requests;
        this.filteredRequests = events.requests;

        if (query != null && query !== '')
        {
          this.filteredEvents = this.filteredEvents.filter(evt => evt.eventName?.toLowerCase().includes(query.toLowerCase()));
          this.filteredRequests = this.filteredRequests.filter(evt => evt.eventName?.toLowerCase().includes(query.toLowerCase()));
        }

        this.finishedEventsInList = this.filteredEvents.some(evt => evt.eventState === EventState.Finished);
        if (!showFinishedEvents && this.finishedEventsInList)
        {
          this.filteredEvents = this.filteredEvents.filter(evt => evt.eventState !== EventState.Finished);
        }

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });

    combineLatest([this.searchEventService.events$, this.query$, this.showOtherFinishedEvents$])
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe(([events, query, showFinishedEvents]) =>
      {
        this.otherEvents = events;
        this.filteredOtherEvents = events;

        if (query != null && query !== '')
        {
          this.filteredOtherEvents = this.filteredOtherEvents.filter(evt => evt.name?.toLowerCase().includes(query.toLowerCase()));
        }

        this.finishedOtherEventsInList = this.filteredOtherEvents.some(evt => evt.state === EventState.Finished);
        if (!showFinishedEvents && this.finishedOtherEventsInList)
        {
          this.filteredOtherEvents = this.filteredOtherEvents.filter(evt => evt.state !== EventState.Finished);
        }

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });
  }

  filterByQuery(query: string): void
  {
    this.query$.next(query);
  }

  trackByFn(index: number, item: any): any
  {
    return item.id || index;
  }

  showFinishedEvents(): void
  {
    this.showFinishedEvents$.next(true);
  }

  hideFinishedEvents(): void
  {
    this.showFinishedEvents$.next(false);
  }

  showOtherFinishedEvents(): void
  {
    this.showOtherFinishedEvents$.next(true);
  }

  hideOtherFinishedEvents(): void
  {
    this.showOtherFinishedEvents$.next(false);
  }

  requestAccess(eventId: string): void
  {
    this.eventsOfUserService.requestAccess(eventId);
  }

  createSuccessorEvent(event: EventOfUser): void
  {
    const dialogRef = this.matDialog.open(CreateEventComponent, { data: { event } });

    dialogRef.afterClosed()
      .subscribe((result) =>
      {
        console.log('Create event dialog was closed!');
      });
  }

  onSubmit(): void
  {
    this.createEventService.createEvent(this.createNewEventForm.value);
  }

  // setRoleOfUser(change: MatSelectChange, userId: string)
  // {
  //   const role = change.value as UserInEventRole;
  //   this.userRolesService.setRoleOfUserInEvent(userId, role);
  // }

  // approveRequest(requestId: string)
  // {
  //   this.accessRequestService.approveRequest(requestId);
  // }

  // denyRequest(requestId: string)
  // {
  //   this.accessRequestService.denyRequest(requestId);
  // }
}
