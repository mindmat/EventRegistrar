import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';
import { EventService } from "../events/eventService.service";
import { Guid } from '../infrastructure/guid';

@Component({
  selector: 'registrables',
  templateUrl: './registrables.component.html'
})
export class RegistrablesComponent implements OnInit {
  newSingleRegistrableId: string;
  newDoubleRegistrableId: string;
  constructor(private http: HttpClient, private route: ActivatedRoute, public readonly eventService: EventService) {
  }

  ngOnInit() {
    this.refresh();
  }

  private refresh() {
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
    }, error => console.error(error));
  }

  changeSingleRegistrableLimits() {
    var payload = {
      maximumParticipants: this.singleRegistrableLimits.maximumParticipants
    };
    this.http.put(`api/events/${this.getEventAcronym()}/registrables/${this.singleRegistrableLimits.id}/singleLimits`, payload).subscribe(result => {
    }, error => console.error(error));
  }
  doubleRegistrables: DoubleRegistrable[];
  singleRegistrables: SingleRegistrable[];
  doubleRegistrableLimits = new DoubleRegistrableLimits();
  singleRegistrableLimits = new SingleRegistrableLimits();

  deleteTestDataAndOpen() {
    this.http.post(`api/events/${this.getEventAcronym()}/openRegistration?deleteTestData=true`, null).subscribe(result => {
      this.refresh();
    }, error => console.error(error));
  }

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }

  deleteRegistrable(registrableId: string) {
    this.http.delete(`api/events/${this.getEventAcronym()}/registrables/${registrableId}`).subscribe(result => {
      this.refresh();
    }, error => console.error(error));
  }

  createNewDoubleRegistrable(name: string) {
    if (this.newDoubleRegistrableId == null) {
      this.newDoubleRegistrableId = Guid.newGuid();
    }
    var params = {
      id: this.newDoubleRegistrableId,
      name: name,
      isDoubleRegistrable: true
    };
    this.http.put(`api/events/${this.getEventAcronym()}/registrables/${this.newDoubleRegistrableId}`, params).subscribe(result => {
      this.newDoubleRegistrableId = null;
      this.refresh();
    }, error => console.error(error));
  }

  createNewSingleRegistrable(name: string) {
    if (this.newSingleRegistrableId == null) {
      this.newSingleRegistrableId = Guid.newGuid();
    }
    var params = {
      id: this.newSingleRegistrableId,
      name: name,
      isDoubleRegistrable: false
    };
    this.http.put(`api/events/${this.getEventAcronym()}/registrables/${this.newSingleRegistrableId}`, params).subscribe(result => {
      this.newSingleRegistrableId = null;
      this.refresh();
    }, error => console.error(error));

  }
}

export class Registrable {
  id: string;
  name: string;
  isDeletable: boolean;
  automaticPromotionFromWaitingList: boolean;
}
export class DoubleRegistrable extends Registrable {
  spotsAvailable: number;
  leadersAccepted: number;
  followersAccepted: number;
  leadersOnWaitingList: number;
  followersOnWaitingList: number;
  maximumAllowedImbalance: number;
}

export class SingleRegistrable extends Registrable {
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
