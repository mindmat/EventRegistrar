import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-waiting-list',
  templateUrl: './waiting-list.component.html'
})
export class WaitingListComponent implements OnInit {
  spots: WaitingListSpot[];

  constructor(private readonly http: HttpClient, private readonly route: ActivatedRoute) {
  }

  ngOnInit() {
    this.refreshParticipants();
  }

  refreshParticipants() {
    this.http.get<WaitingListSpot[]>(`api/events/${this.getEventAcronym()}/registrationsOnWaitingList`)
      .subscribe(result => { this.spots = result; },
        error => console.error(error));
  }

  sendOptions(registrationId: string) {
    console.info(registrationId);
    if (registrationId == null) {
      return;
    }

    var url = `api/events/${this.getEventAcronym()}/registrations/${registrationId}/mails/create?mailType=101&withhold=true`;
    console.info(url);
    this.http.post(url, null)
      .subscribe(result => {  }, error => console.error(error));
  }

  getEventAcronym() {
    return this.route.snapshot.params["eventAcronym"];
  }
}

class WaitingListSpot {
  leader: Registration;
  follower: Registration;
  isOnWaitingList: boolean;
  isPartnerRegistration: boolean;
  placeholderPartner: string;
  registrableName: string;
  joined: Date;
}
class Registration {
  id: string;
  firstName: string;
  lastName: string;
  state: number;
  optionsSent: boolean;
}
