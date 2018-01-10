import { Component, Inject, } from '@angular/core';
import { Http } from '@angular/http';
import { Router, ActivatedRoute, ParamMap } from '@angular/router';

@Component({
  selector: 'participants',
  templateUrl: './participants.component.html'
})
export class ParticipantsComponent {
  public registrable: Registrable;
  private selectedId: number;

  constructor(private http: Http, @Inject('BASE_URL') private baseUrl: string, private router: Router, private route: ActivatedRoute) {
  }

  ngOnInit() {
    var registrableId = this.route.snapshot.params['id'];
    this.http.get(this.baseUrl + 'api/registrables/' + registrableId + '/participants').subscribe(result => {
      //http.get(baseUrl + 'api/registrables/{registrableId}/participants').subscribe(result => {
      this.registrable = result.json() as Registrable;
    }, error => console.error(error));
  }
}

interface Registrable {
  Name: string;
  MaximumSingleSeats: number;
  MaximumDoubleSeats: number;
  MaximumAllowedImbalance: number;
  HasWaitingList: boolean;
  Participants: Place[];
  //ParticipantsOnWaitingList: number;
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