import { Injectable } from '@angular/core';
import { Api, EventSearchResult } from 'app/api/api';
import { Observable } from 'rxjs';
import { FetchService } from '../../infrastructure/fetchService';

@Injectable({
  providedIn: 'root'
})
export class SearchEventsService extends FetchService<EventSearchResult[]>
{
  constructor(private api: Api)
  {
    super();
  }

  get events$(): Observable<EventSearchResult[]>
  {
    return this.result$;
  }

  fetchEvents(): Observable<any>
  {
    return this.fetchItems(this.api.searchEvent_Query({ includeAuthorizedEvents: false }));
  }
}