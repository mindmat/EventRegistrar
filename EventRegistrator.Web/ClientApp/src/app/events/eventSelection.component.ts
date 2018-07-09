import { Component, Inject, } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, ActivatedRoute } from '@angular/router';
import { EventsService } from "./events.service";

@Component({
  selector: 'eventSelection',
  templateUrl: './eventSelection.component.html'
})
export class EventSelectionComponent {
  events: UserInEventDisplayItem[];
  furtherEvents: Event[];
  isSearching: boolean;

  constructor(private http: HttpClient, private eventsService: EventsService) {
  }

  search(searchString: string) {
    this.isSearching = true;
    this.http.get<Event[]>(`api/events?searchstring=${searchString}`)
      .subscribe(result => {
        this.furtherEvents = result;
        this.isSearching = false;
      },
        error => {
          console.error(error);
          this.isSearching = false;
        });
  }

  requestAccess(event: Event) {
    event.requestSent = true;
    this.http.post(`api/events/${event.acronym}/requestAccess`, null)
      .subscribe(result => {
      },
        error => {
          event.requestSent = false;
          console.error(error);
        });
  }

  ngOnInit() {
    this.http.get<UserInEventDisplayItem[]>("api/me/events?includeRequestedEvents=true").subscribe(result => {
      this.events = result;
    },
      error => console.error(error));
  }
}

export class UserInEventDisplayItem {
  eventAcronym: string;
  eventName: string;
  eventState: string;
  role: string;
  userFirstName: string;
  userIdentifier: string;
  userLastName: string;
}

class Event {
  id: string;
  acronym: string;
  name: string;
  state: string;
  requestSent: boolean;
}
