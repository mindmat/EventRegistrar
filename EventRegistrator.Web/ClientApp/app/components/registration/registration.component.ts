import { Component, Inject, } from '@angular/core';
import { Http } from '@angular/http';
import { Router, ActivatedRoute } from '@angular/router';

@Component({
  selector: 'registration',
  templateUrl: './registration.component.html'
})
export class RegistrationComponent {
  public registration: Registration;

  constructor(private http: Http, @Inject('BASE_URL') private baseUrl: string, private router: Router, private route: ActivatedRoute) {
  }

  ngOnInit() {
    var registrationId = this.route.snapshot.params['id'];
    this.http.get(`${this.baseUrl}api/registrations/${registrationId}`).subscribe(result => {
      this.registration = result.json() as Registration;
    }, error => console.error(error));
  }
}

interface Registration {
  Id: string;
  Email: string;
  FirstName: string;
  LastName: string;
  Language: string;
  IsWaitingList: boolean;
  Price: number;
  Paid: number;
  Status: string;
  ReceivedAt: Date;
  ReminderLevel: number;
  SoldOutMessage: string;
  LeaderSpots: Spot[];
  Seats_AsFollower: Spot[];
}

interface Spot {
  Id: string;
  IsCancelled: boolean;
  IsWaitingList: boolean;
  RegistrableName: string;
  RegistrationId: string;
  RegistrationId_Follower: string;
  PartnerEmail: string;
  FirstPartnerJoined: Date;
}