import { Injectable } from '@angular/core';
import
{
  Router, Resolve,
  RouterStateSnapshot,
  ActivatedRouteSnapshot
} from '@angular/router';
import { Observable, of, zip } from 'rxjs';
import { EventsOfUserService } from './events-of-user.service';

@Injectable({
  providedIn: 'root'
})
export class SelectEventResolver implements Resolve<boolean>
{
  constructor(private eventsOfUserService: EventsOfUserService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
  {
    return zip
      (
        this.eventsOfUserService.fetchEventsOfUser()
      );
  }
}
