import { Injectable } from '@angular/core';
import { Api, HostingOffersAndRequests } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../../events/event.service';
import { FetchService } from '../../infrastructure/fetchService';
import { NotificationService } from '../../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class HostingOverviewService extends FetchService<HostingOffersAndRequests>
{
  constructor(private api: Api,
    private eventService: EventService,
    notificationService: NotificationService) 
  {
    super('HostingQuery', notificationService);
  }

  get hosting$(): Observable<HostingOffersAndRequests>
  {
    return this.result$;
  }

  fetchHosting(): Observable<HostingOffersAndRequests>
  {
    return this.fetchItems(this.api.hosting_Query({ eventId: this.eventService.selectedId }), null, this.eventService.selectedId);
  }
}
