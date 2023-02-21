import { Injectable } from '@angular/core';
import { Api, DifferencesDisplayItem } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../../events/event.service';
import { FetchService } from '../../infrastructure/fetchService';
import { NotificationService } from '../../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class PaymentDifferencesService extends FetchService<DifferencesDisplayItem[]> {

  constructor(private api: Api,
    private eventService: EventService,
    notificationService: NotificationService)
  {
    super('DifferencesQuery', notificationService);
  }

  get differences$(): Observable<DifferencesDisplayItem[]>
  {
    return this.result$;
  }

  fetchDifferences()
  {
    return this.fetchItems(this.api.differences_Query({ eventId: this.eventService.selectedId }), null, this.eventService.selectedId);
  }

  refundDifference(registrationId: string, reason: string | null = null)
  {
    return this.api.refundDifference_Command({ eventId: this.eventService.selectedId, registrationId, reason })
      .subscribe();
  }

  sendPleasePayDifferenceMail(registrationId: string)
  {
    return this.api.sendPaymentDueMail_Command({ eventId: this.eventService.selectedId, registrationId })
      .subscribe();
  }
}
