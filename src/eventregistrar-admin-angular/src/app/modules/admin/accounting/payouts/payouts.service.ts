import { Injectable } from '@angular/core';
import { FetchService } from '../../infrastructure/fetchService';
import { Api, PayoutDisplayItem } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../../events/event.service';
import { NotificationService } from '../../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class PayoutsService extends FetchService<PayoutDisplayItem[]> {

  constructor(private api: Api,
    private eventService: EventService,
    notificationService: NotificationService)
  {
    super('PayoutQuery', notificationService);
  }

  get payouts$(): Observable<PayoutDisplayItem[]>
  {
    return this.result$;
  }

  fetchPayouts()
  {
    return this.fetchItems(this.api.payout_Query({ eventId: this.eventService.selectedId }), null, this.eventService.selectedId);
  }
}
