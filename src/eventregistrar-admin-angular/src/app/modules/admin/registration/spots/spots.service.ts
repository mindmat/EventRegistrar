import { Injectable } from '@angular/core';
import { Api, SpotDisplayItem } from 'app/api/api';
import { BehaviorSubject, map, Observable } from 'rxjs';
import { EventService } from '../../events/event.service';

@Injectable({
  providedIn: 'root'
})
export class SpotsService
{
  private spots: BehaviorSubject<SpotDisplayItem[] | null> = new BehaviorSubject(null);

  constructor(private api: Api, private eventService: EventService) { }

  get spots$(): Observable<SpotDisplayItem[]>
  {
    return this.spots.asObservable();
  }

  fetchSpotsOfRegistration(registrationId: string): Observable<SpotDisplayItem[]>
  {
    return this.api.spotsOfRegistration_Query({ eventId: this.eventService.selectedId, registrationId }).pipe(
      map(reg =>
      {
        this.spots.next(reg);
        return reg;
      })
    );
  }
}
