import { Injectable } from '@angular/core';
import { Api, RegistrableDisplayInfo, Role } from 'app/api/api';
import { Observable, Subscription } from 'rxjs';
import { EventService } from '../events/event.service';
import { FetchService } from '../infrastructure/fetchService';
import { NotificationService } from '../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class ParticipantsService extends FetchService<RegistrableDisplayInfo>
{
  constructor(private api: Api,
    private eventService: EventService,
    notificationService: NotificationService)
  {
    super('ParticipantsOfRegistrableQuery', notificationService);
  }

  get registrable$(): Observable<RegistrableDisplayInfo>
  {
    return this.result$;
  }

  fetchParticipantsOf(registrableId: string): Observable<RegistrableDisplayInfo>
  {
    return this.fetchItems(this.api.participantsOfRegistrable_Query({ eventId: this.eventService.selectedId, registrableId }), registrableId, this.eventService.selectedId);
  }

  triggerMoveUp(registrableId: string): Subscription
  {
    return this.api.triggerMoveUpFromWaitingList_Command({ eventId: this.eventService.selectedId, registrableId })
      .subscribe();
  }

  switchRole(registrableId: string, registrationId: string, toRole: Role): Subscription
  {
    return this.api.switchRoleOfParticipant_Command({ eventId: this.eventService.selectedId, registrableId, registrationId, toRole })
      .subscribe();
  }

  promoteFromWaitingList(registrableId: string, registrationId: string): Subscription
  {
    return this.api.triggerMoveUpFromWaitingList_Command({ eventId: this.eventService.selectedId, registrableId, registrationId })
      .subscribe();
  }

  defrag(registrableId: string): Subscription
  {
    return this.api.defragRegistrable_Command({ eventId: this.eventService.selectedId, registrableId })
      .subscribe();
  }
}
