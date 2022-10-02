import { Injectable } from '@angular/core';
import { Api, EventOfUser } from 'app/api/api';
import { Observable } from 'rxjs';
import { FetchService } from '../../infrastructure/fetchService';
import { NotificationService } from '../../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class EventsOfUserService extends FetchService<EventOfUser[]>
{
  constructor(private api: Api,
    notificationService: NotificationService)
  {
    super('EventsOfUserQuery', notificationService);
  }

  get events$(): Observable<EventOfUser[]>
  {
    return this.result$;
  }

  fetchEventsOfUser(): Observable<any>
  {
    return this.fetchItems(this.api.eventsOfUser_Query({ includeRequestedEvents: true }));
  }
}

