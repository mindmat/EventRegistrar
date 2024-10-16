import { Injectable } from '@angular/core';
import { Resolve, RouterStateSnapshot, ActivatedRouteSnapshot, Router } from '@angular/router';
import { catchError, Observable, throwError, zip } from 'rxjs';
import { UserAccessRequestsService } from './user-access/user-access-requests.service';
import { UserAccessService } from './user-access/user-access.service';
import { UserRolesService } from './user-access/user-roles.service';
import { MailConfigService } from './mail-config/mail-config.service';
import { AccountConfigService } from './account-config/account-config.service';

@Injectable({
  providedIn: 'root'
})
export class EventSettingsResolver implements Resolve<boolean>
{
  constructor(private userAccessService: UserAccessService,
    private accessRequestService: UserAccessRequestsService,
    private userRolesService: UserRolesService,
    private mailConfigService: MailConfigService,
    private accountConfigService: AccountConfigService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
  {
    return zip(
      this.userAccessService.fetchUsersOfEvent(),
      this.accessRequestService.fetchRequestOfEvent(),
      this.userRolesService.fetchRoles(),
      this.mailConfigService.fetchMailConfigs(),
      this.accountConfigService.fetchConfig());
  }
}
