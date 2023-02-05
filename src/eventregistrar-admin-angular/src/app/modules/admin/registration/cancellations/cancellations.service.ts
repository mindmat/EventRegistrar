import { Injectable } from '@angular/core';
import { Api, CancellationDisplayItem } from 'app/api/api';
import { EventService } from '../../events/event.service';
import { NotificationService } from '../../infrastructure/notification.service';
import { FetchService } from '../../infrastructure/fetchService';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class CancellationsService extends FetchService<CancellationDisplayItem[]> {

  constructor(private api: Api,
    private eventService: EventService,
    notificationService: NotificationService)
  {
    super('CancellationsQuery', notificationService);
  }

  get cancellations$(): Observable<CancellationDisplayItem[]>
  {
    return this.result$;
  }

  fetchCancellations()
  {
    return this.fetchItems(this.api.cancellations_Query({ eventId: this.eventService.selectedId }), null, this.eventService.selectedId);
  }
}
