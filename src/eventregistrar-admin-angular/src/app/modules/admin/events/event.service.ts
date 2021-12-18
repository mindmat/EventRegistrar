import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class EventService
{
  private selectedEventAcronymSubject: BehaviorSubject<string | null> = new BehaviorSubject('ll22');

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
}
