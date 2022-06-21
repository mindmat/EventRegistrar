import { Injectable } from '@angular/core';
import { Api, RegistrableDisplayInfo } from 'app/api/api';
import { BehaviorSubject, map, Observable, of, switchMap, throwError } from 'rxjs';
import { EventService } from '../events/event.service';

@Injectable({
  providedIn: 'root'
})
export class ParticipantsService
{
  private registrable: BehaviorSubject<RegistrableDisplayInfo | null> = new BehaviorSubject(null);

  constructor(private api: Api, private eventService: EventService) { }

  get registrable$(): Observable<RegistrableDisplayInfo>
  {
    return this.registrable.asObservable();
  }

  fetchParticipantsOf(registrableId: string): Observable<RegistrableDisplayInfo>
  {
    return this.api.participantsOfRegistrable_Query({ eventId: this.eventService.selectedId, registrableId }).pipe(
      map(registrable =>
      {
        // Update the course
        this.registrable.next(registrable);

        // Return the course
        return registrable;
      }),
      switchMap(registrable =>
      {
        if (!registrable)
        {
          return throwError(() => 'Could not find registrable with id of ' + registrableId + '!');
        }

        return of(registrable);
      })
    );
  }
}
