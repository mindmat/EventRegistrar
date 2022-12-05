import { Injectable } from '@angular/core';
import { Api, PotentialPartnerMatch } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../../events/event.service';
import { FetchService } from '../../infrastructure/fetchService';
import { NotificationService } from '../../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class MatchPartnersService extends FetchService<PotentialPartnerMatch[]>
{
  constructor(private api: Api, private eventService: EventService, notificationService: NotificationService)
  {
    super('RegistrationsWithUnmatchedPartnerQuery', notificationService);
  }

  get unmatchedPartners$(): Observable<PotentialPartnerMatch[]>
  {
    return this.result$;
  }

  fetchUnmatchedPartners(): Observable<PotentialPartnerMatch[]>
  {
    return this.fetchItems(this.api.registrationsWithUnmatchedPartner_Query({ eventId: this.eventService.selectedId }), null, this.eventService.selectedId);
  }
}

