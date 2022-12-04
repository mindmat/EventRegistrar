import { Injectable } from '@angular/core';
import { RegistrationMatch, Api, RegistrationState } from 'app/api/api';
import { Observable } from 'rxjs';
import { BehaviorSubject } from 'rxjs/internal/BehaviorSubject';
import { EventService } from '../../events/event.service';

@Injectable({
  providedIn: 'root'
})
export class SearchRegistrationsForSettlementService
{
  private list: BehaviorSubject<RegistrationMatch[] | null> = new BehaviorSubject(null);
  private minimumSearchQueryLength: number = 2;

  constructor(private api: Api, private eventService: EventService) { }

  get matches$(): Observable<RegistrationMatch[]>
  {
    return this.list.asObservable();
  }

  fetchItemsOf(searchString: string)
  {
    if (searchString?.length >= this.minimumSearchQueryLength)
    {
      this.api.searchRegistration_Query({ eventId: this.eventService.selectedId, searchString, states: [RegistrationState.Received, RegistrationState.Paid, RegistrationState.Cancelled] })
        .subscribe(newItems =>
        {
          this.list.next(newItems);
        });
    }
    else
    {
      this.list.next(null);
    }
  }
}
