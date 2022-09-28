import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { MatSelectChange } from '@angular/material/select';
import { RoleDescription, UserInEventDisplayItem, UserInEventRole } from 'app/api/api';
import { BehaviorSubject, combineLatest, Subject, takeUntil } from 'rxjs';
import { UserAccessService } from './user-access.service';
import { UserRolesService } from './user-roles.service';

@Component({
  selector: 'app-user-access',
  templateUrl: './user-access.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UserAccessComponent implements OnInit
{
  private unsubscribeAll: Subject<any> = new Subject<any>();
  usersWithAccess: UserInEventDisplayItem[];
  filteredUsersWithAccess: UserInEventDisplayItem[];
  query$: BehaviorSubject<string | null> = new BehaviorSubject(null);
  roles: RoleDescription[];

  constructor(private userAccessService: UserAccessService,
    private userRolesService: UserRolesService,
    private changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void
  {
    combineLatest([this.userAccessService.usersWithAccess$, this.query$])
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe(([users, query]) =>
      {
        this.usersWithAccess = users;
        this.filteredUsersWithAccess = users;

        if (query != null && query !== '')
        {
          this.filteredUsersWithAccess = this.filteredUsersWithAccess.filter(usr => usr.userDisplayName.toLowerCase().includes(query.toLowerCase()));
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
}
