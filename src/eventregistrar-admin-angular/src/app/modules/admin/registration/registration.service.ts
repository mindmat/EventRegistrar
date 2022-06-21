import { Injectable } from '@angular/core';
import { Api, RegistrationDisplayItem } from 'app/api/api';
import { BehaviorSubject, map, Observable } from 'rxjs';
import { EventService } from '../events/event.service';

@Injectable({
  providedIn: 'root'
})
export class RegistrationService
{
  private registration: BehaviorSubject<RegistrationDisplayItem | null> = new BehaviorSubject(null);

  constructor(private api: Api, private eventService: EventService) { }

  get registration$(): Observable<RegistrationDisplayItem>
  {
    return this.registration.asObservable();
  }

  fetchRegistration(registrationId: string): Observable<RegistrationDisplayItem>
  {
    return this.api.registration_Query({ eventId: this.eventService.selectedId, registrationId: registrationId }).pipe(
      map(reg =>
      {
        this.registration.next(reg);
        return reg;
      })
    );
  }
}