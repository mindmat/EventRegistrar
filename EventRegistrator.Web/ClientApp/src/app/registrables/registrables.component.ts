import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'registrables',
  templateUrl: './registrables.component.html'
})
export class RegistrablesComponent implements OnInit {
  ngOnInit() {
    this.http.get<DoubleRegistrable[]>(`api/events/${this.getEventAcronym()}/DoubleRegistrableOverview`).subscribe(result => {
      this.doubleRegistrables = result;
    }, error => console.error(error));
    this.http.get<SingleRegistrable[]>(`api/events/${this.getEventAcronym()}/SingleRegistrableOverview`).subscribe(result => {
      this.singleRegistrables = result;
    }, error => console.error(error));
  }

  doubleRegistrables: DoubleRegistrable[];
  singleRegistrables: SingleRegistrable[];

  constructor(private http: HttpClient, private route: ActivatedRoute) {
  }

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }
}

class DoubleRegistrable {
  id: string;
  name: string;
  spotsAvailable: number;
  leadersAccepted: number;
  followersAccepted: number;
  leadersOnWaitingList: number;
  followersOnWaitingList: number;
}

class SingleRegistrable {
  id: string;
  name: string;
  spotsAvailable: number;
  accepted: number;
  onWaitingList: number;
}
