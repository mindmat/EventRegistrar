import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, map, Observable, of, switchMap, throwError } from 'rxjs';
import { EventService } from '../events/event.service';

@Injectable({
  providedIn: 'root'
})
export class ParticipantsDoubleService
{
  private registrable: BehaviorSubject<RegistrableWithParticipants | null> = new BehaviorSubject(null);

  constructor(private httpClient: HttpClient, private eventService: EventService) { }

  get registrable$(): Observable<RegistrableWithParticipants>
  {
    return this.registrable.asObservable();
  }

  fetchParticipantsOf(registrableId: string): Observable<RegistrableWithParticipants>
  {
    return this.httpClient.get<RegistrableWithParticipants>(`api/events/${this.eventService.selected}/registrables/${registrableId}/participants`).pipe(
      map(registrable =>
      {
        // Update the course
        this.registrable.next(registrable);

        // Return the course
        return registrable;
      }),
      switchMap(course =>
      {
        if (!course)
        {
          return throwError(() => 'Could not find course with id of ' + registrableId + '!');
        }

        return of(course);
      })
    );
  }
}


export class RegistrableWithParticipants
{
  name: string;
  nameSecondary: string;
  maximumSingleSeats: number;
  maximumDoubleSeats: number;
  maximumAllowedImbalance: number;
  hasWaitingList: boolean;
  automaticPromotionFromWaitingList: boolean;
  participants: Spot[];
  waitingList: Spot[];
  acceptedLeaders: number;
  acceptedFollowers: number;
  leadersOnWaitingList: number;
  followersOnWaitingList: number;
}

export class Spot
{
  leader?: Registration;
  follower?: Registration;
  isOnWaitingList: boolean;
  isPartnerRegistration: boolean;
  placeholderPartner?: string;
  joined: Date;
}

export class Registration
{
  id: string;
  firstName: string;
  lastName: string;
  state: number;
  email: string;
}