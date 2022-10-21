import { Injectable } from '@angular/core';
import { Api, RegistrableDisplayItem } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../../events/event.service';
import { FetchService } from '../../infrastructure/fetchService';
import { NotificationService } from '../../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class RegistrablesService extends FetchService<RegistrableDisplayItem[]>
{
  constructor(private api: Api,
    notificationService: NotificationService,
    private eventService: EventService)
  {
    super('RegistrablesQuery', notificationService);
  }

  get registrables$(): Observable<RegistrableDisplayItem[]>
  {
    return this.result$;
  }

  fetchRegistrables(): Observable<any>
  {
    return this.fetchItems(this.api.registrables_Query({ eventId: this.eventService.selectedId }));
  }
}