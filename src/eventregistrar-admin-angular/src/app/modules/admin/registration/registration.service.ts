import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, map, Observable, switchMap } from 'rxjs';
import { EventService } from '../events/event.service';

@Injectable({
  providedIn: 'root'
})
export class RegistrationService
{
  private registration: BehaviorSubject<Registration | null> = new BehaviorSubject(null);

  constructor(private httpClient: HttpClient, private eventService: EventService) { }

  get registration$(): Observable<Registration>
  {
    return this.registration.asObservable();
  }

  fetchRegistration(registrationId: string): Observable<Registration>
  {
    return this.httpClient.get<Registration>(`api/events/${this.eventService.selected}/registrations/${registrationId}`).pipe(
      map(reg =>
      {
        this.registration.next(reg);
        return reg;
      })
    );
  }
}


export class Registration
{
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  language: string;
  isWaitingList: boolean;
  price: number;
  paid: number;
  status: number;
  statusText: string;
  receivedAt: Date;
  reminderLevel: number;
  soldOutMessage: string;
  fallbackToPartyPass: boolean;
  smsCount: number;
  remarks: string;
  phoneNormalized: string;
  partnerOriginal: string;
  partnerName: string;
  partnerId: string;
  isReduced: boolean;
  willPayAtCheckin: boolean;
}