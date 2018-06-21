import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable()
export class EventsService {
  constructor(private http: HttpClient) {
    //this.http.get<UserInEventDisplayItem[]>("api/me/events").subscribe(result => {
    //  this.events = result;
    //},
    //  error => console.error(error));
  }

  events: UserInEventDisplayItem[];

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
