import { Injectable } from '@angular/core';
import { Api, RegistrationFormItem } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../../events/event.service';
import { FetchService } from '../../infrastructure/fetchService';
import { NotificationService } from '../../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class FormsService extends FetchService<RegistrationFormItem[]>
{
  constructor(private api: Api,
    notificationService: NotificationService,
    private eventService: EventService)
  {
    super('RegistrationFormsQuery', notificationService);
  }

  get forms$(): Observable<RegistrationFormItem[]>
  {
    return this.result$;
  }

  fetchForms(): Observable<any>
  {
    return this.fetchItems(this.api.registrationForms_Query({ eventId: this.eventService.selectedId }));
  }
}