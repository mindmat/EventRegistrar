import { Injectable } from '@angular/core';
import { Api, DoubleRegistrableDisplayItem, RegistrableTagDisplayItem, SingleRegistrableDisplayItem } from 'app/api/api';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { EventService } from '../events/event.service';

@Injectable({
  providedIn: 'root'
})
export class OverviewService
{
  private registrableTags: BehaviorSubject<RegistrableTagDisplayItem[] | null> = new BehaviorSubject(null);
  private singleRegistrables: BehaviorSubject<SingleRegistrableDisplayItem[] | null> = new BehaviorSubject(null);
  private doubleRegistrables: BehaviorSubject<DoubleRegistrableDisplayItem[] | null> = new BehaviorSubject(null);

  constructor(private api: Api, private eventService: EventService) { }

  get registrableTags$(): Observable<RegistrableTagDisplayItem[]>
  {
    return this.registrableTags.asObservable();
  }

  get singleRegistrables$(): Observable<SingleRegistrableDisplayItem[]>
  {
    return this.singleRegistrables.asObservable();
  }

  get doubleRegistrables$(): Observable<DoubleRegistrableDisplayItem[]>
  {
    return this.doubleRegistrables.asObservable();
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

  fetchSingleRegistrables(): Observable<SingleRegistrableDisplayItem[]>
  {
    return this.api.singleRegistrablesOverview_Query({ eventId: this.eventService.selectedId })
      .pipe(
        tap((response: any) =>
        {
          this.singleRegistrables.next(response);
        })
      );
  }

  fetchDoubleRegistrables(): Observable<DoubleRegistrableDisplayItem[]>
  {
    return this.api.doubleRegistrablesOverview_Query({ eventId: this.eventService.selectedId })
      .pipe(
        tap((response: any) =>
        {
          this.doubleRegistrables.next(response);
        })
      );
  }
}
