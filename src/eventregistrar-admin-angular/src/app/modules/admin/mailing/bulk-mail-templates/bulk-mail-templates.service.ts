import { Injectable } from '@angular/core';
import { Api, BulkMailTemplates, MailType } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../../events/event.service';
import { FetchService } from '../../infrastructure/fetchService';
import { NotificationService } from '../../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class BulkMailTemplatesService extends FetchService<BulkMailTemplates> {

  constructor(private api: Api,
    private eventService: EventService,
    notificationService: NotificationService)
  {
    super('BulkMailTemplatesQuery', notificationService);
  }

  get bulkMailTemplates$(): Observable<BulkMailTemplates>
  {
    return this.result$;
  }

  fetchBulkMailTemplates(): Observable<BulkMailTemplates>
  {
    return this.fetchItems(this.api.bulkMailTemplates_Query({ eventId: this.eventService.selectedId }), null, this.eventService.selectedId);
  }

  createTemplate(key: string): void
  {
    this.api.createBulkMailTemplate_Command({ eventId: this.eventService.selectedId, key })
      .subscribe();
  }

  deleteTemplate(key: string): void
  {
    this.api.deleteBulkMailTemplate_Command({ eventId: this.eventService.selectedId, bulkMailKey: key })
      .subscribe();
  }
}
