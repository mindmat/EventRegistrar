import { Injectable } from '@angular/core';
import { Api } from 'app/api/api';
import { EventService } from 'app/modules/admin/events/event.service';
import { BehaviorSubject, Observable, filter, map, switchMap, throwError, of, mergeMap, shareReplay } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class RightsService
{
  private rights: BehaviorSubject<Set<string>> = new BehaviorSubject(new Set<string>());

  constructor(eventService: EventService,
    private api: Api)
  {
    eventService.selectedId$.pipe(
      mergeMap(eventId => api.rightsOfUserInEvent_Query({ eventId: eventId }))
    ).subscribe(
      rights => this.rights.next(new Set<string>(rights))
    );
  }

  public get rights$(): Observable<Set<string>>
  {
    return this.rights.asObservable();
  }
}
