import { Injectable } from '@angular/core';
import { Api, RegistrationMatch, RegistrationState } from 'app/api/api';
import { BehaviorSubject, Observable } from 'rxjs';
import { EventService } from '../../events/event.service';

@Injectable({
  providedIn: 'root'
})
export class SearchRegistrationService
{
  private list: BehaviorSubject<RegistrationMatch[] | null> = new BehaviorSubject(null);

  constructor(private api: Api, private eventService: EventService) { }

  get list$(): Observable<RegistrationMatch[]>
  {
    return this.list.asObservable();
  }

  fetchItemsOf(searchString: string)
  {
    this.api.searchRegistration_Query({ eventId: this.eventService.selectedId, searchString, states: [RegistrationState.Received, RegistrationState.Paid, RegistrationState.Cancelled] })
      .subscribe(newItems =>
      {
        this.list.next(newItems);
      });
  }
}
