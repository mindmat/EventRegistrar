import { Injectable } from '@angular/core';
import { Resolve, RouterStateSnapshot, ActivatedRouteSnapshot, Router } from '@angular/router';
import { catchError, Observable, throwError, zip } from 'rxjs';
import { UserAccessService } from './user-access.service';
import { UserRolesService } from './user-roles.service';

@Injectable({
  providedIn: 'root'
})
export class UserAccessResolver implements Resolve<boolean>
{
  constructor(private userAccessService: UserAccessService,
    private userRolesService: UserRolesService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
  {
    return zip(
      this.userAccessService.fetchUsersOfEvent(),
      this.userRolesService.fetchRoles());
  }
}
