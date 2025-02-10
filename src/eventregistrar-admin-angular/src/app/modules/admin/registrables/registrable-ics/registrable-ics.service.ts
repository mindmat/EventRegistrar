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

  getRegistrableIcs(registrableId: string, registrableIcsId: string): Observable<RegistrableIcsItem>
  {
    return this.api.registrableIcs_Query({ eventId: this.eventService.selectedId, registrableId, registrableIcsId });
  }

  saveRegistrableIcs(registrableId: string, values: RegistrableIcsItem): void
  {
    var command = { eventId: this.eventService.selectedId, registrableId, registrableIcsItem: values } as SaveRegistrableIcsCommand;
    this.api.saveRegistrableIcs_Command(command)
      .subscribe();
  }
}
