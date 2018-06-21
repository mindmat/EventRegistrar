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

  constructor(private http: HttpClient, private eventsService: EventsService) {
    this.http.get<UserInEventDisplayItem[]>("api/me/events").subscribe(result => {
      this.events = result;
    },
      error => console.error(error));
  }

  ngOnInit() {
    //var eventId = '762A93A4-56E0-402C-B700-1CFB3362B39D';
    //this.http.get<Registration[]>(`${this.baseUrl}api/events/${eventId}/checkinView`)
    //  .subscribe(result => { this.registrations = result; },
    //    error => console.error(error));
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
