import { Component, Inject, } from '@angular/core';
import { Http } from '@angular/http';
import { Router, ActivatedRoute } from '@angular/router';

@Component({
  selector: 'participants',
  templateUrl: './participants.component.html'
})
export class ParticipantsComponent {
  public registrable: Registrable;
  registrableId: any;

  constructor(private http: Http, @Inject('BASE_URL') private baseUrl: string, private router: Router, private route: ActivatedRoute) {
  }

  ngOnInit() {
    this.registrableId = this.route.snapshot.params['id'];
    this.refreshParticipants();
  }

  refreshParticipants() {
    this.http.get(`${this.baseUrl}api/registrables/${this.registrableId}/participants`)
      .subscribe(result => { this.registrable = result.json() as Registrable; },
      error => console.error(error));
  }

  promote() {
    this.http.post(`${this.baseUrl}api/registrables/${this.registrableId}/tryPromoteFromWaitingList`, null)
      .subscribe(result => { },
      error => console.error(error));

  }

}

interface Registrable {
  Name: string;
  MaximumSingleSeats: number;
  MaximumDoubleSeats: number;
  MaximumAllowedImbalance: number;
  HasWaitingList: boolean;
  Participants: Place[];
  WaitingList: Place[];
}

interface Place {
  Leader: Registration;
  Follower: Registration;
  IsOnWaitingList: boolean;
  IsPartnerRegistration: boolean;
}

interface Registration {
  Id: string;
  FirstName: string;
  LastName: string;
  State: number;
  Email: string;
}