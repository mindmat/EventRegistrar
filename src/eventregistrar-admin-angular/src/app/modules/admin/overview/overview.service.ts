import { Injectable } from '@angular/core';
import { Api, RegistrablesOverview, RegistrableTagDisplayItem } from 'app/api/api';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { EventService } from '../events/event.service';
import { FetchService } from '../infrastructure/fetchService';
import { NotificationService } from '../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class OverviewService extends FetchService<RegistrablesOverview | null>
{
  private registrableTags: BehaviorSubject<RegistrableTagDisplayItem[] | null> = new BehaviorSubject(null);

  constructor(private api: Api, private eventService: EventService, notificationService: NotificationService)
  {
    super('RegistrablesOverviewQuery', notificationService);
  }

  get registrableTags$(): Observable<RegistrableTagDisplayItem[]>
  {
    return this.registrableTags.asObservable();
  }

  get registrables$(): Observable<RegistrablesOverview>
  {
    return this.result$;
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

  fetchRegistrables(): Observable<any>
  {
    return this.fetchItems(this.api.registrablesOverview_Query({ eventId: this.eventService.selectedId }), null, this.eventService.selectedId);
  }
}
