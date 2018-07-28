import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'eventAuthorization',
  templateUrl: './eventAuthorization.component.html'
})
export class EventAuthorizationComponent implements OnInit {
  events: UserInEventDisplayItem[];
  furtherEvents: Event[];
  isSearching: boolean;

  constructor(private http: HttpClient, private route: ActivatedRoute) {
  }

  ngOnInit() {
    this.refresh();
  }

  refresh() {
    var eventAcronym = this.getEventAcronym();
    this.http.get<UserInEventDisplayItem[]>(`api/events/${eventAcronym}/users`).subscribe(result => {
      this.users = result;
    },
      error => console.error(error));
    this.http.get<AccessRequestOfEvent[]>(`api/events/${eventAcronym}/requests`).subscribe(result => {
      this.requests = result;
    },
      error => console.error(error));
  }

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }

  users: UserInEventDisplayItem[];
  requests: AccessRequestOfEvent[];

  removeUserFromEvent(user: UserInEventDisplayItem) {
    var index = this.users.indexOf(user);
    this.users.splice(index, 1);
    this.http.delete(`api/events/${this.getEventAcronym()}/users/${user.userId}/roles/${user.role}`).subscribe(result => {
      this.refresh();
    });
  }

  grantAccess(request: AccessRequestOfEvent) {
    var index = this.requests.indexOf(request);
    this.requests.splice(index, 1);
    var response = new RequestResponseDto();
    response.response = RequestResponse.Granted;
    response.role = UserInEventRole.Reader;
    this.http.post(`api/events/${this.getEventAcronym()}/accessrequest/${request.id}/respond`, response).subscribe(result => {
      this.refresh();
    });
  }

  denyAccess(request: AccessRequestOfEvent) {
    var index = this.requests.indexOf(request);
    this.requests.splice(index, 1);
    var response = new RequestResponseDto();
    response.response = RequestResponse.Denied;
    this.http.post(`api/events/${this.getEventAcronym()}/accessrequest/${request.id}/respond`, response).subscribe(result => {
      this.refresh();
    });
  }
}

export class UserInEventDisplayItem {
  eventAcronym: string;
  eventName: string;
  eventState: string;
  role: string;
  userId: string;
  userFirstName: string;
  userEmail: string;
  userLastName: string;
}

export class AccessRequestOfEvent {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  requestReceived: string;
  requestText: string;
}

enum RequestResponse {
  Granted = 1,
  Denied = 2
}

enum UserInEventRole {
  None = 0,
  Reader = 1,
  Writer = 2,
  Admin = 3
}

class RequestResponseDto {
  response: RequestResponse;
  role: UserInEventRole;
  text: string;
}
