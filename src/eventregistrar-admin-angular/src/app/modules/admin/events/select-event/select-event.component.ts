import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { AccessRequest, CreateEventCommand, EventOfUser, EventSearchResult, EventState, RoleDescription, UserInEventRole } from 'app/api/api';
import { BehaviorSubject, combineLatest, Subject, takeUntil } from 'rxjs';
import { CreateEventComponent } from './create-event/create-event.component';
import { EventsOfUserService } from './events-of-user.service';
import { SearchEventsService } from './search-events.service';
import { FormBuilder, FormGroup } from '@angular/forms';
import { v4 as createUuid } from 'uuid';
import { CreateEventService } from './create-event/create-event.service';

@Component({
  selector: 'app-select-event',
  templateUrl: './select-event.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
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
