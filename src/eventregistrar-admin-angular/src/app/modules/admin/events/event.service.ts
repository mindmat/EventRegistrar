import { Injectable } from '@angular/core';
import { Api, EventDetails } from 'app/api/api';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { NotificationService } from '../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class EventService
{
  private selectedEventIdSubject: BehaviorSubject<string | null> = new BehaviorSubject(null);
  private selectedEventSubject: BehaviorSubject<EventDetails | null> = new BehaviorSubject(null);
  // private selectedEventAcronymSubject: BehaviorSubject<string | null> = new BehaviorSubject('ll22');
  // private selectedEventIdSubject: BehaviorSubject<string | null> = new BehaviorSubject('40EB7B32-696E-41D5-9A57-AE9A45344E2B');
  // private selectedEventAcronymSubject: BehaviorSubject<string | null> = new BehaviorSubject('bsw2209');
  // private selectedEventIdSubject: BehaviorSubject<string | null> = new BehaviorSubject('0CF31A7C-6CFF-4DD1-AC37-3EE98E713791');
  // private selectedEventAcronymSubject: BehaviorSubject<string | null> = new BehaviorSubject('sb21');
  // private selectedEventIdSubject: BehaviorSubject<string | null> = new BehaviorSubject('BF1D1E9F-259F-404A-A4B3-3FAE03B5942B');

  private cache = new Map<string, EventDetails>();

  constructor(private notificationService: NotificationService, private api: Api)
  {
    this.notificationService.switchToEvent(this.selectedEventIdSubject.value);
    this.selectedId$.subscribe(eventId => this.notificationService.switchToEvent(eventId));
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
      const observable = this.api.eventByAcronym_Query({ eventAcronym });
      observable.subscribe(e =>
      {
        if (e !== null)
        {
          this.cache.set(eventAcronym, e);
        }
        this.setEvent(e);
      });
      return observable;
    }
  }

  private setEvent(eventDetails: EventDetails)
  {
    this.selectedEventSubject.next(eventDetails);
    this.selectedEventIdSubject.next(eventDetails.id);
  }
}
