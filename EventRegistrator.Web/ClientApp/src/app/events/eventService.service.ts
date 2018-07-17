import { Injectable } from '@angular/core';
import { UserInEventDisplayItem } from '../events/eventSelection.component';

@Injectable()
export class EventService {
  public acronym: string;
  public name: string;

  public setEvent(event: UserInEventDisplayItem) {
    this.acronym = event.eventAcronym;
    this.name = event.eventName;
  }
}
