import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { MatSelectChange } from '@angular/material/select';
import { AccessRequestOfEvent, RoleDescription, UserInEventDisplayItem, UserInEventRole } from 'app/api/api';
import { BehaviorSubject, combineLatest, Subject, takeUntil } from 'rxjs';
import { UserAccessRequestsService } from './user-access-requests.service';
import { UserAccessService } from './user-access.service';
import { UserRolesService } from './user-roles.service';

@Component({
  selector: 'app-user-access',
  templateUrl: './user-access.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UserAccessComponent implements OnInit
{
  usersWithAccess: UserInEventDisplayItem[];
  filteredUsersWithAccess: UserInEventDisplayItem[];
  requests: AccessRequestOfEvent[];
  filteredRequests: AccessRequestOfEvent[];
  query$: BehaviorSubject<string | null> = new BehaviorSubject(null);
  roles: RoleDescription[];
  private unsubscribeAll: Subject<any> = new Subject<any>();

  constructor(private userAccessService: UserAccessService,
    private accessRequestService: UserAccessRequestsService,
    private userRolesService: UserRolesService,
    private changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void
  {
    combineLatest([this.userAccessService.usersWithAccess$, this.accessRequestService.requests$, this.query$])
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe(([users, requests, query]) =>
      {
        this.usersWithAccess = users;
        this.filteredUsersWithAccess = users;
        this.requests = requests;
        this.filteredRequests = requests;

        if (query != null && query !== '')
        {
          this.filteredUsersWithAccess = this.filteredUsersWithAccess.filter(usr => usr.userDisplayName.toLowerCase().includes(query.toLowerCase()));
          this.filteredRequests = this.filteredRequests.filter(req =>
            req.firstName?.toLowerCase().includes(query.toLowerCase())
            || req.lastName?.toLowerCase().includes(query.toLowerCase())
            || req.email?.toLowerCase().includes(query.toLowerCase()));
        }

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });

    this.userRolesService.roles$.subscribe((roles) =>
    {
      this.roles = roles;
      this.changeDetectorRef.markForCheck();
    });
  }

  filterUsers(query: string): void
  {
    this.query$.next(query);
  }

  trackByFn(index: number, item: any): any
  {
    return item.id || index;
  }

  lookupRoleName(role: UserInEventRole): string
  {
    return this.roles.find(rol => rol.role === role)?.name;
  }

  removeUser(userId: string): void
  {
    this.userRolesService.removeUserFromEvent(userId);
  }

  setRoleOfUser(change: MatSelectChange, userId: string): void
  {
    const role = change.value as UserInEventRole;
    this.userRolesService.setRoleOfUserInEvent(userId, role);
  }

  approveRequest(requestId: string): void
  {
    this.accessRequestService.approveRequest(requestId);
  }

  denyRequest(requestId: string): void
  {
    this.accessRequestService.denyRequest(requestId);
  }
}
