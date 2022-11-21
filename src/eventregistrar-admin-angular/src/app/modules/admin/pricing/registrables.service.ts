import { Injectable } from '@angular/core';
import { Api, RegistrableDisplayItem, RegistrableTypeOption, SaveRegistrableCommand } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../events/event.service';
import { FetchService } from '../infrastructure/fetchService';
import { NotificationService } from '../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class RegistrablesService extends FetchService<RegistrableDisplayItem[]> {

  constructor(private api: Api,
    private eventService: EventService,
    notificationService: NotificationService)
  {
    super('RegistrablesQuery', notificationService);
  }

  get registrables$(): Observable<RegistrableDisplayItem[]>
  {
    return this.result$;
  }

  fetchRegistrables(): Observable<RegistrableDisplayItem[]>
  {
    return this.fetchItems(this.api.registrables_Query({ eventId: this.eventService.selectedId }), null, this.eventService.selectedId);
  }

  getRegistrableTypes(): Observable<RegistrableTypeOption[]>
  {
    return this.api.registrableTypes_Query({});
  }

  saveRegistrable(command: SaveRegistrableCommand)
  {
    throw this.api.saveRegistrable_Command(command).subscribe();
  }

  deleteRegistrable(registrableId: string)
  {
    throw this.api.deleteRegistrable_Command({ eventId: this.eventService.selectedId, registrableId }).subscribe();
  }
}