import { Injectable } from '@angular/core';
import { UserInEventDisplayItem } from '../events/eventSelection.component';

@Injectable()
export class EventService {
  constructor() { }
  public acronym: string;
  public name: string;

  public setEvent(event: UserInEventDisplayItem) {
    console.info(event.eventAcronym);
    this.acronym = event.eventAcronym;
    this.name = event.eventName;
  }
}
