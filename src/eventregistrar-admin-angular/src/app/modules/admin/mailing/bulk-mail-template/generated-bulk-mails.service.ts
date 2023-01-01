import { Injectable } from '@angular/core';
import { Api, GeneratedBulkMails } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../../events/event.service';
import { FetchService } from '../../infrastructure/fetchService';
import { NotificationService } from '../../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class GeneratedBulkMailsService extends FetchService<GeneratedBulkMails>
{
  constructor(private api: Api,
    private eventService: EventService,
    notificationService: NotificationService)
  {
    super('GeneratedBulkMailsQuery', notificationService);
  }

  get generated$(): Observable<GeneratedBulkMails>
  {
    return this.result$;
  }

  fetchMailCount(bulkMailKey: string)
  {
    return this.fetchItems(this.api.generatedBulkMails_Query({ eventId: this.eventService.selectedId, bulkMailKey }), null, this.eventService.selectedId);
  }
}