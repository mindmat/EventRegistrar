import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class EventService
{
  private selectedEventAcronymSubject: BehaviorSubject<string | null> = new BehaviorSubject('ll22');
  private selectedEventIdSubject: BehaviorSubject<string | null> = new BehaviorSubject('40EB7B32-696E-41D5-9A57-AE9A45344E2B');

  constructor() { }

  get selected$(): Observable<string>
  {
    return this.selectedEventAcronymSubject.asObservable();
  }
  get selected(): string | null
  {
    return this.selectedEventAcronymSubject.value;
  }

  set selected(eventAcronym: string)
  {
    this.selectedEventAcronymSubject.next(eventAcronym);
  }

  get selectedId$(): Observable<string>
  {
    return this.selectedEventIdSubject.asObservable();
  }
  get selectedId(): string | null
  {
    return this.selectedEventIdSubject.value;
  }

  set selectedId(eventAcronym: string)
  {
    this.selectedEventIdSubject.next(eventAcronym);
  }
}
