import { Injectable } from '@angular/core';
import { Api } from 'app/api/api';
import { EventService } from '../../events/event.service';

@Injectable({
  providedIn: 'root'
})
export class ChangeNameService
{

  constructor(private api: Api,
    private eventService: EventService) { }

  cancelRegistration(registrationId: string, firstName: string, lastName: string)
  {
    this.api.changeParticipantName_Command({ eventId: this.eventService.selectedId, registrationId, firstName, lastName })
      .subscribe();
  }
}
