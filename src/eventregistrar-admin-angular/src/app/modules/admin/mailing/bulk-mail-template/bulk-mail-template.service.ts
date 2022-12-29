import { Injectable } from '@angular/core';
import { Api, BulkMailTemplateDisplayItem, PlaceholderDescription } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../../events/event.service';
import { FetchService } from '../../infrastructure/fetchService';
import { NotificationService } from '../../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class BulkMailTemplateService extends FetchService<BulkMailTemplateDisplayItem> {

  private bulkMailTemplateId: string;

  constructor(private api: Api,
    private eventService: EventService,
    notificationService: NotificationService)
  {
    super('BulkMailTemplateQuery', notificationService);
  }

  get template$(): Observable<BulkMailTemplateDisplayItem>
  {
    return this.result$;
  }

  fetchTemplate(bulkMailTemplateId: string)
  {
    this.bulkMailTemplateId = bulkMailTemplateId;
    return this.fetchItems(this.api.bulkMailTemplate_Query({ eventId: this.eventService.selectedId, bulkMailTemplateId }), this.bulkMailTemplateId, this.eventService.selectedId);
  }

  getAvailablePlaceholders(): Observable<PlaceholderDescription[]>
  {
    return this.api.bulkMailPlaceholder_Query({ eventId: this.eventService.selectedId });
  }
}
