import { Injectable } from '@angular/core';
import { Api, RemarksDisplayItem } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../../events/event.service';
import { FetchService } from '../../infrastructure/fetchService';
import { NotificationService } from '../../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class RemarksOverviewService extends FetchService<RemarksDisplayItem[] | null>
{
  constructor(private api: Api, private eventService: EventService, notificationService: NotificationService)
  {
    super('RemarksOverviewQuery', notificationService);
  }

  get remarks$(): Observable<RemarksDisplayItem[]>
  {
    return this.result$;
  }

  fetchRemarks(): Observable<any>
  {
    return this.fetchItems(this.api.remarksOverview_Query({ eventId: this.eventService.selectedId }), null, this.eventService.selectedId);
  }
}
