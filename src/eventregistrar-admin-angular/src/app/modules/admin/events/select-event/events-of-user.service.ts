import { Injectable } from '@angular/core';
import { Api, EventOfUser, EventsOfUser } from 'app/api/api';
import { Observable } from 'rxjs';
import { FetchService } from '../../infrastructure/fetchService';
import { NotificationService } from '../../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class EventsOfUserService extends FetchService<EventsOfUser>
{
  constructor(private api: Api,
    notificationService: NotificationService)
  {
    super('EventsOfUserQuery', notificationService);
  }

  get events$(): Observable<EventsOfUser>
  {
    return this.result$;
  }

  fetchEventsOfUser(): Observable<any>
  {
    return this.fetchItems(this.api.eventsOfUser_Query({ includeRequestedEvents: true }));
  }

  requestAccess(eventId: string)
  {
    return this.api.requestAccess_Command({ eventId })
      .subscribe();
  }
}

