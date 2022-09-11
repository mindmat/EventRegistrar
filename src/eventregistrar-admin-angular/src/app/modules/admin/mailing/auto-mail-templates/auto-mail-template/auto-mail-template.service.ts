import { Injectable } from '@angular/core';
import { Api, AutoMailTemplateDisplayItem } from 'app/api/api';
import { EventService } from 'app/modules/admin/events/event.service';
import { FetchService } from 'app/modules/admin/infrastructure/fetchService';
import { NotificationService } from 'app/modules/admin/infrastructure/notification.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AutoMailTemplateService extends FetchService<AutoMailTemplateDisplayItem> {

  private autoMailTemplateId: string;

  constructor(private api: Api,
    private eventService: EventService,
    notificationService: NotificationService)
  {
    super('AutoMailTemplateQuery', notificationService);
  }

  get template$(): Observable<AutoMailTemplateDisplayItem>
  {
    return this.result$;
  }

  fetchTemplate(autoMailTemplateId: string)
  {
    this.autoMailTemplateId = autoMailTemplateId;
    return this.fetchItems(this.api.autoMailTemplate_Query({ eventId: this.eventService.selectedId, mailTemplateId: autoMailTemplateId }), this.autoMailTemplateId);
  }
}
