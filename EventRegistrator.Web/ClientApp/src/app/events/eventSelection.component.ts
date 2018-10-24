import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { EventService } from '../events/eventService.service';
import { Guid } from '../infrastructure/guid';

@Component({
  selector: 'eventSelection',
  templateUrl: './eventSelection.component.html'
})
export class EventSelectionComponent {
  events: UserInEventDisplayItem[];
  furtherEvents: Event[];
  isSearching: boolean;
  newEvent: Event;
  newEventCopyOfEventId: string;
  newEventCopyOfEventName: string;

  constructor(private http: HttpClient, private eventService: EventService) {
    this.newEvent = new Event();
    this.newEvent.id = Guid.newGuid();
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

  showEvent(event: UserInEventDisplayItem) {
    this.newEvent.name = event.eventName;
    this.newEvent.acronym = event.eventAcronym;
    this.newEventCopyOfEventName = `${event.eventName} (${event.eventAcronym})`;
    this.newEventCopyOfEventId = event.eventId;
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

  setEvent(event: UserInEventDisplayItem) {
    this.eventService.setEvent(event);
  }

  createEvent() {
    var url = `api/events/${this.newEvent.acronym}?name=${this.newEvent.name}&id=${this.newEvent.id}`;
    if (this.newEventCopyOfEventId != null) {
      url += `&eventId_CopyFrom=${this.newEventCopyOfEventId}`;
    }
    this.http.put(url, null).subscribe(result => {
    },
      error => console.error(error));
  }

  ngOnInit() {
    this.http.get<UserInEventDisplayItem[]>("api/me/events?includeRequestedEvents=true").subscribe(result => {
      this.events = result;
    },
      error => console.error(error));
  }
}

export class UserInEventDisplayItem {
  eventId: string;
  eventAcronym: string;
  eventName: string;
  eventState: number;
  role: number;
  userFirstName: string;
  userIdentifier: string;
  userLastName: string;
}

export class Event {
  id: string;
  acronym: string;
  name: string;
  state: string;
  requestSent: boolean;
}
