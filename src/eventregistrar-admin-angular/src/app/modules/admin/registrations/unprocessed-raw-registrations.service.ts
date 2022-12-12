import { Injectable } from '@angular/core';
import { Api, UnprocessedRawRegistrationsInfo } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../events/event.service';
import { FetchService } from '../infrastructure/fetchService';
import { NotificationService } from '../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class UnprocessedRawRegistrationsService extends FetchService<UnprocessedRawRegistrationsInfo> {

  constructor(private api: Api, private eventService: EventService, notificationService: NotificationService)
  {
    super('UnprocessedRawRegistrationCountQuery', notificationService);
  }

  get unprocessedRawRegistrationsInfo$(): Observable<UnprocessedRawRegistrationsInfo>
  {
    return this.result$;
  }

  fetchInfo(): Observable<UnprocessedRawRegistrationsInfo>
  {
    return this.fetchItems(this.api.unprocessedRawRegistrationCount_Query({ eventId: this.eventService.selectedId }), null, this.eventService.selectedId);
  }

  startProcessAllPendingRawRegistrationsCommand()
  {
    this.api.startProcessAllPendingRawRegistrations_Command({ eventId: this.eventService.selectedId })
      .subscribe();
  }
}
