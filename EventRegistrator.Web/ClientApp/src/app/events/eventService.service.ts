import { Injectable } from '@angular/core';
import { UserInEventDisplayItem } from '../events/eventSelection.component';

@Injectable()
export class EventService {
  constructor() { }
  public eventAcronym: string;

  public setEvent(event: UserInEventDisplayItem) {
    console.info(event.eventAcronym);
    this.eventAcronym = event.eventAcronym;
  }
}
