import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, map, Observable, of, switchMap, throwError } from 'rxjs';
import { EventService } from '../../events/event.service';

@Injectable({
  providedIn: 'root'
})
export class SearchRegistrationService
{
  private list: BehaviorSubject<RegistrationMatch[] | null> = new BehaviorSubject(null);

  constructor(private httpClient: HttpClient, private eventService: EventService) { }

  get list$(): Observable<RegistrationMatch[]>
  {
    return this.list.asObservable();
  }

  fetchItemsOf(searchString: string)
  {
    const url = `api/events/${this.eventService.selected}/registrations?searchstring=${searchString}&states=received&states=paid&states=cancelled`;
    this.httpClient.get<RegistrationMatch[]>(url).subscribe(newItems =>
    {
      this.list.next(newItems);
    });
  }
}


export class RegistrationMatch
{
  registrationId: string;
  email: string;
  firstName: string;
  lastName: string;
  receivedAt: Date;
  language: string;
  price: number;
  amountPaid: number;
  state: number;
  stateText: string;
  isWaitingList: boolean;
  spots: RegistrableName[];
}

export class RegistrableName
{
  name: string;
  secondary?: string;
  isWaitingList: boolean;
}