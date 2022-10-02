import { Injectable } from '@angular/core';
import { Resolve, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { Observable, of } from 'rxjs';
import { EventService } from './event.service';

@Injectable({
  providedIn: 'root'
})
export class EventAcronymResolver implements Resolve<boolean>
{
  constructor(private eventService: EventService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
  {
    return this.eventService.setEventByAcronym(route.paramMap.get('eventAcronym'));
  }
}
