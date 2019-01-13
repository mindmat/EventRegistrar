import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';
import { Place, Registration } from "../participants/participants.component";

@Component({
  selector: 'app-waiting-list',
  templateUrl: './waiting-list.component.html'
})
export class WaitingListComponent implements OnInit {
  spots: Place[];

  constructor(private readonly http: HttpClient, private readonly route: ActivatedRoute) {
  }

  ngOnInit() {
    this.refreshParticipants();
  }

  refreshParticipants() {
    this.http.get<Place[]>(`api/events/${this.getEventAcronym()}/registrationsOnWaitingList`)
      .subscribe(result => { this.spots = result; },
        error => console.error(error));
  }

  getEventAcronym() {
    return this.route.snapshot.params["eventAcronym"];
  }
}
