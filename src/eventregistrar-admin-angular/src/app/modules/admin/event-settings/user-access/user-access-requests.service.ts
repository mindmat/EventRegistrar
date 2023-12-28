import { Injectable } from '@angular/core';
import { Api, AccessRequestOfEvent, RequestResponse } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../../events/event.service';
import { FetchService } from '../../infrastructure/fetchService';
import { NotificationService } from '../../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class UserAccessRequestsService extends FetchService<AccessRequestOfEvent[]>
{
  constructor(private api: Api,
    private eventService: EventService,
    notificationService: NotificationService)
  {
    super('AccessRequestsOfEventQuery', notificationService);
  }

  get requests$(): Observable<AccessRequestOfEvent[]>
  {
    return this.result$;
  }

  fetchRequestOfEvent(): Observable<AccessRequestOfEvent[]>
  {
    return this.fetchItems(this.api.accessRequestsOfEvent_Query({ eventId: this.eventService.selectedId }), null, this.eventService.selectedId);
  }

  approveRequest(requestId: string): void
  {
    this.api.respondToRequest_Command({ eventId: this.eventService.selectedId, accessToEventRequestId: requestId, response: RequestResponse.Granted })
      .subscribe();
  }

  denyRequest(requestId: string): void
  {
    this.api.respondToRequest_Command({ eventId: this.eventService.selectedId, accessToEventRequestId: requestId, response: RequestResponse.Denied })
      .subscribe();
  }
}
