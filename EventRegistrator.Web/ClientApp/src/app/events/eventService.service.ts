import { Injectable } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { HttpClient } from '@angular/common/http';

@Injectable()
export class EventService {
  public activeEvent: EventDetails;

  constructor(private readonly router: Router,
    private readonly titleService: Title,
    private readonly http: HttpClient) {
    this.router.events.subscribe(event => {
      if (event instanceof NavigationEnd && this.activeEvent == null) {
        var eventAcronym = event.url.split("/").filter(split => split !== "")[0];
        this.setActiveEvent(eventAcronym);
      }
    });
  }

  setActiveEvent(eventAcronym: string) {
    if (eventAcronym != null && this.activeEvent == null) {
      this.http.get<EventDetails>(`api/events/${eventAcronym}`).subscribe(result => {
        this.activeEvent = result;
        this.titleService.setTitle(result.name);
      }, error => console.error(error));
    }
  }
}

class EventDetails {
  id: string;
  name: string;
  acronym: string;
  state: EventState;
}

enum EventState {
  Setup = 1,
  RegistrationOpen = 2,
  RegistrationClosed = 3,
  Finished = 4
}
