import { Injectable } from '@angular/core';
import { Api, DuePaymentItem } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../../events/event.service';
import { FetchService } from '../../infrastructure/fetchService';
import { NotificationService } from '../../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class DuePaymentsService extends FetchService<DuePaymentItem[]> {

  constructor(private api: Api,
    private eventService: EventService,
    notificationService: NotificationService)
  {
    super('DuePaymentsQuery', notificationService);
  }

  get duePayments$(): Observable<DuePaymentItem[]>
  {
    return this.result$;
  }

  fetchDuePayments()
  {
    return this.fetchItems(this.api.duePayments_Query({ eventId: this.eventService.selectedId }), null, this.eventService.selectedId);
  }

  sendReminderMail(registrationId: string)
  {
    return this.api.sendReminderMail_Command({ eventId: this.eventService.selectedId, registrationId })
      .subscribe();
  }
}
