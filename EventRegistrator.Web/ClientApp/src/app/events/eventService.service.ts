import { Injectable } from '@angular/core';
import { UserInEventDisplayItem } from '../events/eventSelection.component';
import { Router, NavigationEnd } from '@angular/router';

@Injectable()
export class EventService {
  public acronym: string;
  public name: string;

  constructor(private router: Router) {
    this.router.events.subscribe(event => {
      if (event instanceof NavigationEnd && this.acronym == null) {
        var eventAcronym = event.url.split("/").filter(split => split !== "")[0];
        this.setAcronymIfNotSetYet(eventAcronym);
      }
    });
  }

  public setEvent(event: UserInEventDisplayItem) {
    this.acronym = event.eventAcronym;
    this.name = event.eventName;
  }

  setAcronymIfNotSetYet(eventAcronym: string) {
    if (eventAcronym != null && this.acronym == null) {
      this.acronym = eventAcronym;
    }
  }
}
