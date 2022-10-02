import { Injectable } from '@angular/core';
import { Resolve, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { Observable, zip } from 'rxjs';
import { EventsOfUserService } from './events-of-user.service';
import { SearchEventsService } from './search-events.service';

@Injectable({
  providedIn: 'root'
})
export class SelectEventResolver implements Resolve<boolean>
{
  constructor(private eventsOfUserService: EventsOfUserService,
    private searchEventsService: SearchEventsService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
  {
    return zip
      (
        this.eventsOfUserService.fetchEventsOfUser(),
        this.searchEventsService.fetchEvents()
      );
  }
}
