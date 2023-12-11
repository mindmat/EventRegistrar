import { Injectable } from '@angular/core';
import { Api, MailDeliverySuccess } from 'app/api/api';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { EventService } from '../../events/event.service';
import { FetchService } from '../../infrastructure/fetchService';
import { NotificationService } from '../../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class MailDeliverySuccessService extends FetchService<MailDeliverySuccess> {

  private sentSinceDays: number | null;

  constructor(private api: Api,
    private eventService: EventService,
    notificationService: NotificationService)
  {
    super('MailDeliverySuccessQuery', notificationService);
  }

  get stats$(): Observable<MailDeliverySuccess>
  {
    return this.result$;
  }

  fetchStats(sentSinceDays: number | null = null): Observable<MailDeliverySuccess>
  {
    this.sentSinceDays = sentSinceDays;
    return this.fetchItems(this.api.mailDeliverySuccess_Query({ eventId: this.eventService.selectedId, sentSinceDays: this.sentSinceDays }), null, this.eventService.selectedId);
  }
};
