import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, map, Observable } from 'rxjs';
import { EventService } from '../../events/event.service';

@Injectable({
  providedIn: 'root'
})
export class SpotsService
{
  private spots: BehaviorSubject<SpotOfRegistration[] | null> = new BehaviorSubject(null);

  constructor(private httpClient: HttpClient, private eventService: EventService) { }

  get spots$(): Observable<SpotOfRegistration[]>
  {
    return this.spots.asObservable();
  }

  fetchSpotsOfRegistration(registrationId: string): Observable<SpotOfRegistration[]>
  {
    return this.httpClient.get<SpotOfRegistration[]>(`api/events/${this.eventService.selected}/registrations/${registrationId}/spots`).pipe(
      map(reg =>
      {
        this.spots.next(reg);
        return reg;
      })
    );
  }
}

export type RegistrableType = 'single' | 'double';

export class SpotOfRegistration
{
  id: string;
  registrableId: string;
  registrableName: string;
  registrableNameSecondary: string;
  roleText: string;
  partnerRegistrationId: string;
  firstPartnerJoined: Date;
  partnerName: string;
  isCore: boolean;
  isWaitingList: boolean;
  type: RegistrableType;
}