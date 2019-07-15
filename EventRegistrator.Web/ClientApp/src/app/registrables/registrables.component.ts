import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';
import { EventService } from "../events/eventService.service";

@Component({
  selector: 'registrables',
  templateUrl: './registrables.component.html'
})
export class RegistrablesComponent implements OnInit {
  constructor(private http: HttpClient, private route: ActivatedRoute, private eventService: EventService) {
  }

  ngOnInit() {
    this.http.get<DoubleRegistrable[]>(`api/events/${this.getEventAcronym()}/DoubleRegistrableOverview`).subscribe(result => {
      this.doubleRegistrables = result;
    }, error => console.error(error));
    this.http.get<SingleRegistrable[]>(`api/events/${this.getEventAcronym()}/SingleRegistrableOverview`).subscribe(result => {
      this.singleRegistrables = result;
    }, error => console.error(error));
  }

  editCoupleLimits(registrable: DoubleRegistrable) {
    this.doubleRegistrableLimits.id = registrable.id;
    this.doubleRegistrableLimits.name = registrable.name;
    this.doubleRegistrableLimits.spotsAvailable = registrable.spotsAvailable;
    this.doubleRegistrableLimits.maximumAllowedImbalance = registrable.maximumAllowedImbalance;
  }

  editSingleLimits(registrable: SingleRegistrable) {
    this.singleRegistrableLimits.id = registrable.id;
    this.singleRegistrableLimits.name = registrable.name;
    this.singleRegistrableLimits.maximumParticipants = registrable.spotsAvailable;
  }

  changeCoupleRegistrableLimits() {
    var payload = {
      maximumCouples: this.doubleRegistrableLimits.spotsAvailable,
      maximumImbalance: this.doubleRegistrableLimits.maximumAllowedImbalance
    };
    this.http.put(`api/events/${this.getEventAcronym()}/registrables/${this.doubleRegistrableLimits.id}/coupleLimits`, payload).subscribe(result => {
      //this.doubleRegistrables = result;
    }, error => console.error(error));;
  }

  changeSingleRegistrableLimits() {
    var payload = {
      maximumParticipants: this.singleRegistrableLimits.maximumParticipants
    };
    this.http.put(`api/events/${this.getEventAcronym()}/registrables/${this.singleRegistrableLimits.id}/singleLimits`, payload).subscribe(result => {
      //this.doubleRegistrables = result;
    }, error => console.error(error));;
  }
  doubleRegistrables: DoubleRegistrable[];
  singleRegistrables: SingleRegistrable[];
  doubleRegistrableLimits = new DoubleRegistrableLimits();
  singleRegistrableLimits = new SingleRegistrableLimits();

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }
}

export class Registrable {
  id: string;
  name: string;
}
export class DoubleRegistrable extends Registrable{
  //id: string;
  //name: string;
  spotsAvailable: number;
  leadersAccepted: number;
  followersAccepted: number;
  leadersOnWaitingList: number;
  followersOnWaitingList: number;
  maximumAllowedImbalance: number;
}

export class SingleRegistrable extends Registrable {
  //id: string;
  //name: string;
  spotsAvailable: number;
  accepted: number;
  onWaitingList: number;
}

class DoubleRegistrableLimits {
  id: string;
  name: string;
  spotsAvailable: number;
  maximumAllowedImbalance: number;
}

class SingleRegistrableLimits {
  id: string;
  name: string;
  maximumParticipants: number;
}
