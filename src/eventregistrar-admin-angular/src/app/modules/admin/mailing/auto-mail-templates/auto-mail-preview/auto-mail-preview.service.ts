import { Injectable } from '@angular/core';
import { AutoMailTemplateDisplayItem, Api, AutoMailPreview } from 'app/api/api';
import { EventService } from 'app/modules/admin/events/event.service';
import { FetchService } from 'app/modules/admin/infrastructure/fetchService';
import { NotificationService } from 'app/modules/admin/infrastructure/notification.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AutoMailPreviewService extends FetchService<AutoMailPreview> {

  private autoMailTemplateId: string;

  constructor(private api: Api,
    private eventService: EventService,
    notificationService: NotificationService)
  {
    super('AutoMailPreviewQuery', notificationService);
  }

  get preview$(): Observable<AutoMailPreview>
  {
    return this.result$;
  }

  fetchPreview(autoMailTemplateId: string, registrationId: string)
  {
    this.autoMailTemplateId = autoMailTemplateId;
    return this.fetchItems(this.api.autoMailPreview_Query({ eventId: this.eventService.selectedId, autoMailTemplateId, registrationId }), this.autoMailTemplateId);
  }
}