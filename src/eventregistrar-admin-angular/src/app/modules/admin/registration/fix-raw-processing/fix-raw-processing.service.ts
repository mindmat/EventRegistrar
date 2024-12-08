import { Injectable } from '@angular/core';
import { FetchService } from '../../infrastructure/fetchService';
import { Api, ProcessingError, Role } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../../events/event.service';
import { NotificationService } from '../../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class FixRawProcessingService extends FetchService<ProcessingError[] | null>
{
  constructor(private api: Api, private eventService: EventService, notificationService: NotificationService)
  {
    super('ProcessingErrorsQuery', notificationService);
  }

  get errors$(): Observable<ProcessingError[]>
  {
    return this.result$;
  }

  fetchErrors(): Observable<any>
  {
    return this.fetchItems(this.api.processingErrors_Query({ eventId: this.eventService.selectedId }), null, this.eventService.selectedId);
  }

  fixRole(rawRegistrationId: string, role: Role)
  {
    return this.api.fixMissingRole_Command({ eventId: this.eventService.selectedId, rawRegistrationId, role })
      .subscribe();
  }

  retry(rawRegistrationId: string)
  {
    return this.api.processRawRegistration_Command({ rawRegistrationId })
      .subscribe();
  }
}
