import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { MatSelectChange } from '@angular/material/select';
import { EventOfUser, RoleDescription, UserInEventRole } from 'app/api/api';
import { BehaviorSubject, combineLatest, Subject, takeUntil } from 'rxjs';
import { UserRolesService } from '../../auth/user-access/user-roles.service';
import { EventsOfUserService } from './events-of-user.service';

@Component({
  selector: 'app-select-event',
  templateUrl: './select-event.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SelectEventComponent implements OnInit
{
  private unsubscribeAll: Subject<any> = new Subject<any>();
  events: EventOfUser[];
  filteredEvents: EventOfUser[];
  // requests: AccessRequestOfEvent[];
  // filteredRequests: AccessRequestOfEvent[];
  query$: BehaviorSubject<string | null> = new BehaviorSubject(null);
  roles: RoleDescription[];

  constructor(private eventsOfUserService: EventsOfUserService,
    // private accessRequestService: UserAccessRequestsService,
    private userRolesService: UserRolesService,
    private changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void
  {
    combineLatest([this.eventsOfUserService.events$, /*this.accessRequestService.requests$,*/ this.query$])
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe(([events, /*requests,*/ query]) =>
      {
        this.events = events;
        this.filteredEvents = events;
        // this.requests = requests;
        // this.filteredRequests = requests;

        if (query != null && query !== '')
        {
          this.filteredEvents = this.filteredEvents.filter(evt => evt.eventName?.toLowerCase().includes(query.toLowerCase()));
          // this.filteredRequests = this.filteredRequests.filter(req =>
          // req.firstName?.toLowerCase().includes(query.toLowerCase())
          // || req.lastName?.toLowerCase().includes(query.toLowerCase())
          // || req.email?.toLowerCase().includes(query.toLowerCase()));
        }

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });

    this.userRolesService.roles$.subscribe(roles =>
    {
      this.roles = roles;
      this.changeDetectorRef.markForCheck();
    });
  }

  filterUsers(query: string)
  {
    this.query$.next(query);
  }

  trackByFn(index: number, item: any): any
  {
    return item.id || index;
  }

  lookupRoleName(role: UserInEventRole): string
  {
    return this.roles.find(rol => rol.role == role)?.name;
  }

  removeUser(userId: string)
  {
    this.userRolesService.removeUserFromEvent(userId);
  }

  setRoleOfUser(change: MatSelectChange, userId: string)
  {
    const role = change.value as UserInEventRole;
    this.userRolesService.setRoleOfUserInEvent(userId, role);
  }

  // approveRequest(requestId: string)
  // {
  //   this.accessRequestService.approveRequest(requestId);
  // }

  // denyRequest(requestId: string)
  // {
  //   this.accessRequestService.denyRequest(requestId);
  // }
}
