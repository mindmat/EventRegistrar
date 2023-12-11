import { Injectable } from '@angular/core';
import { Api, ProblematicEmail, RegistrationState } from 'app/api/api';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { EventService } from '../../events/event.service';

@Injectable({
  providedIn: 'root'
})
export class ProblematicEmailsService
{
  private list: BehaviorSubject<ProblematicEmail[] | null> = new BehaviorSubject(null);

  constructor(private api: Api, private eventService: EventService) { }

  get list$(): Observable<ProblematicEmail[]>
  {
    return this.list.asObservable();
  }

  fetchItems(searchString: string | null = null): Observable<ProblematicEmail[]>
  {
    return this.api.notReceivedMails_Query({ eventId: this.eventService.selectedId, searchString })
      .pipe(
        tap(newItems => this.list.next(newItems))
      );
  }
}
