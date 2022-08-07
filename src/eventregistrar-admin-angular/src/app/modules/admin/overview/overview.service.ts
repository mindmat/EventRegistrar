import { Injectable } from '@angular/core';
import { Api, DoubleRegistrableDisplayItem, RegistrablesOverview, RegistrableTagDisplayItem, SingleRegistrableDisplayItem } from 'app/api/api';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { EventService } from '../events/event.service';

@Injectable({
  providedIn: 'root'
})
export class OverviewService
{
  private registrableTags: BehaviorSubject<RegistrableTagDisplayItem[] | null> = new BehaviorSubject(null);
  private registrables: BehaviorSubject<RegistrablesOverview | null> = new BehaviorSubject(null);

  constructor(private api: Api, private eventService: EventService) { }

  get registrableTags$(): Observable<RegistrableTagDisplayItem[]>
  {
    return this.registrableTags.asObservable();
  }

  get registrables$(): Observable<RegistrablesOverview>
  {
    return this.registrables.asObservable();
  }

  fetchRegistrableTags(): Observable<RegistrableTagDisplayItem[]>
  {
    return this.api.registrableTags_Query({ eventId: this.eventService.selectedId })
      .pipe(
        tap((response: any) =>
        {
          this.registrableTags.next(response);
        })
      );
  }

  fetchRegistrables(): Observable<RegistrablesOverview>
  {
    return this.api.registrablesOverview_Query({ eventId: this.eventService.selectedId })
      .pipe(
        tap((response: any) =>
        {
          this.registrables.next(response);
        })
      );
  }
}
