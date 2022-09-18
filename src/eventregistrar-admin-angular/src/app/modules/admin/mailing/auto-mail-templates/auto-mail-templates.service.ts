import { Injectable } from '@angular/core';
import { Api, AutoMailTemplates, MailType } from 'app/api/api';
import { map } from 'lodash';
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

  createTemplate(type: MailType, language: string): Observable<string>
  {
    return this.api.createAutoMailTemplate_Command({ eventId: this.eventService.selectedId, type, language });
  }

  updateSettings(senderMail: string, senderName: string, availableLanguages: string[], singleRegistrationPossible: boolean, partnerRegistrationPossible: boolean)
  {
    this.api.updateAutoMailConfiguration_Command({ eventId: this.eventService.selectedId, senderMail, senderName, availableLanguages, singleRegistrationPossible, partnerRegistrationPossible })
      .subscribe(x => console.log(x));
  }
}
