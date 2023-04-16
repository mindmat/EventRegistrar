import { Injectable } from '@angular/core';
import { Api, EventDetails } from 'app/api/api';
import { BehaviorSubject, Observable, filter, of, tap } from 'rxjs';
import { NotificationService } from '../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class EventService
{
  private selectedEventIdSubject: BehaviorSubject<string | null> = new BehaviorSubject(null);
  private selectedEventSubject: BehaviorSubject<EventDetails | null> = new BehaviorSubject(null);
  private cache = new Map<string, EventDetails>();

  constructor(private notificationService: NotificationService, private api: Api)
  {
    this.notificationService.switchToEvent(this.selectedEventIdSubject.value);
    this.selectedId$.subscribe(eventId => this.notificationService.switchToEvent(eventId));
    this.notificationService.subscribe('EventByAcronymQuery').pipe(
      filter(e => e.eventId === this.selectedId || e.eventId?.toLowerCase() === this.selectedId?.toLowerCase())
    ).subscribe(_ => this.refresh());
  }

  get selected$(): Observable<EventDetails>
  {
    return this.selectedEventSubject.asObservable();
  }
  get selected(): EventDetails | null
  {
    return this.selectedEventSubject.value;
  }

  get selectedId$(): Observable<string>
  {
    return this.selectedEventIdSubject.asObservable();
  }
  get selectedId(): string | null
  {
    return this.selectedEventIdSubject.value;
  }

  setEventByAcronym(eventAcronym: string | null): Observable<any>
  {
    if (eventAcronym === null)
    {
      return of(null);
    }
    eventAcronym = eventAcronym.toLowerCase();
    if (this.cache.has(eventAcronym))
    {
      const e = this.cache.get(eventAcronym);
      this.setEvent(e);
      return of(null);
    }
    else
    {
      return this.api.eventByAcronym_Query({ eventAcronym })
        .pipe(
          tap(e =>
          {
            if (e !== null)
            {
              this.cache.set(eventAcronym, e);
            }
            this.setEvent(e);
          })
        );
    }
  }

  refresh(): void
  {
    var eventAcronym = this.selected.acronym;
    if (!!eventAcronym)
    {
      this.api.eventByAcronym_Query({ eventAcronym })
        .subscribe(result =>
        {
          if (result !== null)
          {
            this.cache.set(eventAcronym, result);
          }
          this.setEvent(result);
        });
    }
  }

  private setEvent(eventDetails: EventDetails)
  {
    this.selectedEventSubject.next(eventDetails);
    this.selectedEventIdSubject.next(eventDetails.id);
  }
}
