import { Injectable } from '@angular/core';
import { FetchService } from '../../infrastructure/fetchService';
import { Api, ExternalMailConfigurationDisplayItem, ExternalMailConfigurationUpdateItem, UserInEventDisplayItem } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../../events/event.service';
import { NotificationService } from '../../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class MailConfigService extends FetchService<ExternalMailConfigurationDisplayItem[]>
{
  constructor(private api: Api,
    private eventService: EventService,
    notificationService: NotificationService)
  {
    super('ExternalMailConfigurationQuery', notificationService);
  }

  get mailConfigs$(): Observable<ExternalMailConfigurationDisplayItem[]>
  {
    return this.result$;
  }

  fetchMailConfigs(): Observable<any>
  {
    return this.fetchItems(this.api.externalMailConfiguration_Query({ eventId: this.eventService.selectedId }), null, this.eventService.selectedId);
  }

  save(configs: ExternalMailConfigurationUpdateItem[]): void
  {
    this.api.saveExternalMailConfiguration_Command({ eventId: this.eventService.selectedId, configs })
      .subscribe();
  }
}
