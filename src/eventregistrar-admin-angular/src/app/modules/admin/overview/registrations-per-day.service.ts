import { Injectable } from '@angular/core';
import { Api, RegistrationsPerDay } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../events/event.service';
import { FetchService } from '../infrastructure/fetchService';
import { NotificationService } from '../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class RegistrationsPerDayService extends FetchService<RegistrationsPerDay[] | null>
{
  constructor(private api: Api, private eventService: EventService, notificationService: NotificationService)
  {
    super('RegistrationsPerDayQuery', notificationService);
  }

  get registrationsPerDay$(): Observable<RegistrationsPerDay[] | null>
  {
    return this.result$;
  }

  fetchData(): Observable<any>
  {
    return this.fetchItems(this.api.registrationsPerDay_Query({ eventId: this.eventService.selectedId }), null, this.eventService.selectedId);
  }
}
