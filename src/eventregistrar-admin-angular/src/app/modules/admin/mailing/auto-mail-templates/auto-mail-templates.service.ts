import { Injectable } from '@angular/core';
import { Api, AutoMailTemplates } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../../events/event.service';
import { FetchService } from '../../infrastructure/fetchService';
import { NotificationService } from '../../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class AutoMailTemplatesService extends FetchService<AutoMailTemplates> {

  constructor(private api: Api,
    private eventService: EventService,
    notificationService: NotificationService)
  {
    super('AutoMailTemplatesQuery', notificationService);
  }

  get autoMailTemplates$(): Observable<AutoMailTemplates>
  {
    return this.result$;
  }

  fetchAutoMailTemplates()
  {
    return this.fetchItems(this.api.autoMailTemplates_Query({ eventId: this.eventService.selectedId, }));
  }
}
