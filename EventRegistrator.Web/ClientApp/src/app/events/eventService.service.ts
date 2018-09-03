import { Injectable } from '@angular/core';
import { UserInEventDisplayItem } from '../events/eventSelection.component';
import { Router, ActivatedRoute, NavigationEnd } from '@angular/router';

@Injectable()
export class EventService {
  public acronym: string;
  public name: string;

  constructor(private router: Router, private readonly route: ActivatedRoute) {
    this.router.events.subscribe(event => {
      if (event instanceof NavigationEnd) {
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
