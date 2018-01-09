import { Component, Inject } from '@angular/core';
import { Http } from '@angular/http';

@Component({
  selector: 'registrables',
  templateUrl: './registrables.component.html'
})
export class RegistrablesComponent {
  public doubleRegistrables: DoubleRegistrable[];
  public singleRegistrables: SingleRegistrable[];

  constructor(http: Http, @Inject('BASE_URL') baseUrl: string) {
    http.get(baseUrl + 'api/DoubleRegistrableOverview').subscribe(result => {
      this.doubleRegistrables = result.json() as DoubleRegistrable[];
    }, error => console.error(error));
    http.get(baseUrl + 'api/SingleRegistrableOverview').subscribe(result => {
      this.singleRegistrables = result.json() as SingleRegistrable[];
    }, error => console.error(error));
  }
}

interface DoubleRegistrable {
  Name: string;
  SpotsAvailable: number;
  LeadersAccepted: number;
  FollowersAccepted: number;
  LeadersOnWaitingList: number;
  FollowersOnWaitingList: number;
}

interface SingleRegistrable {
  Name: string;
  SpotsAvailable: number;
  Accepted: number;
  OnWaitingList: number;
}