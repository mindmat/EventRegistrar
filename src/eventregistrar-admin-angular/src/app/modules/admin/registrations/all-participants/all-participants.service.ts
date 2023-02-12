import { Injectable } from '@angular/core';
import { Api, RegistrationState, Participant } from 'app/api/api';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { EventService } from '../../events/event.service';

@Injectable({
  providedIn: 'root'
})
export class AllParticipantsService
{
  private list: BehaviorSubject<Participant[] | null> = new BehaviorSubject(null);

  constructor(private api: Api, private eventService: EventService) { }

  get list$(): Observable<Participant[]>
  {
    return this.list.asObservable();
  }

  fetchItemsOf(searchString: string): Observable<Participant[]>
  {
    return this.api.participantsOfEvent_Query({ eventId: this.eventService.selectedId, searchString, states: [RegistrationState.Received, RegistrationState.Paid] })
      .pipe(
        tap(newItems => this.list.next(newItems))
      );
  }
}
