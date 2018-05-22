import { Component, Inject } from '@angular/core';
import { Http } from '@angular/http';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'registrables',
  templateUrl: './registrables.component.html'
})
export class RegistrablesComponent {
  public doubleRegistrables: DoubleRegistrable[];
  public singleRegistrables: SingleRegistrable[];

  constructor(http: Http, @Inject('BASE_URL') baseUrl: string, private route: ActivatedRoute) {
    http.get(`${baseUrl}api/${this.getEventAcronym()}/DoubleRegistrableOverview`).subscribe(result => {
      this.doubleRegistrables = result.json() as DoubleRegistrable[];
    }, error => console.error(error));
    http.get(`${baseUrl}api/${this.getEventAcronym()}/SingleRegistrableOverview`).subscribe(result => {
      this.singleRegistrables = result.json() as SingleRegistrable[];
    }, error => console.error(error));
  }

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }
}

interface DoubleRegistrable {
  Id: string;
  Name: string;
  SpotsAvailable: number;
  LeadersAccepted: number;
  FollowersAccepted: number;
  LeadersOnWaitingList: number;
  FollowersOnWaitingList: number;
}

interface SingleRegistrable {
  Id: string;
  Name: string;
  SpotsAvailable: number;
  Accepted: number;
  OnWaitingList: number;
}