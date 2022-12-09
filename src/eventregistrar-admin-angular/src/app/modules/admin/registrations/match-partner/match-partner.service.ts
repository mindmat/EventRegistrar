import { Injectable } from '@angular/core';
import { Api, PotentialPartners } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../../events/event.service';
import { FetchService } from '../../infrastructure/fetchService';
import { NotificationService } from '../../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class MatchPartnerService extends FetchService<PotentialPartners>
{
  registrationId: string;

  constructor(private api: Api,
    private eventService: EventService,
    notificationService: NotificationService)
  {
    super('PotentialPartnersQuery', notificationService);
  }

  fetchCandidates(registrationId: string, searchString: string | null = null): Observable<any>
  {
    this.registrationId = registrationId;
    return this.fetchItems(this.api.potentialPartners_Query({ eventId: this.eventService.selectedId, registrationId, searchString }), this.registrationId, this.eventService.selectedId);
  }

  get candidates$(): Observable<PotentialPartners>
  {
    return this.result$;
  }

  assign(registrationId1: string, registrationId2: string)
  {
    this.api.matchPartnerRegistrations_Command({ eventId: this.eventService.selectedId, registrationId1, registrationId2 })
      .subscribe(x => console.log(x));
  }

  transformToSingle(registrationId: string)
  {
    this.api.changeUnmatchedPartnerRegistrationToSingleRegistration_Command({ eventId: this.eventService.selectedId, registrationId })
      .subscribe(x => console.log(x));
  }
}
