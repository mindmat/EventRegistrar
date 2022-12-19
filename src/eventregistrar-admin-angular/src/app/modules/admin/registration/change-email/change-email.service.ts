import { Injectable } from '@angular/core';
import { Api } from 'app/api/api';
import { EventService } from '../../events/event.service';

@Injectable({
  providedIn: 'root'
})
export class ChangeEmailService
{
  constructor(private api: Api,
    private eventService: EventService) { }

  cancelRegistration(registrationId: string, oldEmailAddress: string, newEmailAddress: string)
  {
    this.api.fixInvalidAddress_Command({ eventId: this.eventService.selectedId, registrationId, oldEmailAddress, newEmailAddress })
      .subscribe();
  }
}
