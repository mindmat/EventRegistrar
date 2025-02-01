import { Injectable } from '@angular/core';
import { Api, RegistrableIcsItem, SaveRegistrableIcsCommand } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../../events/event.service';

@Injectable({
  providedIn: 'root'
})
export class RegistrableIcsService
{
  constructor(private api: Api,
    private eventService: EventService) { }

  getRegistrableIcs(registrableId: string): Observable<RegistrableIcsItem | null>
  {
    return this.api.registrableIcs_Query({ eventId: this.eventService.selectedId, registrableId });
  }

  saveRegistrableIcs(values: RegistrableIcsItem): void
  {
    var command = { eventId: this.eventService.selectedId, registrableIcsItem: values } as SaveRegistrableIcsCommand;
    this.api.saveRegistrableIcs_Command(command)
      .subscribe();
  }
}
