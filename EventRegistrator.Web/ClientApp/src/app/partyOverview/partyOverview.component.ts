import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'partyOverview',
  templateUrl: './partyOverview.component.html'
})
export class PartyOverviewComponent {
  parties: Party[];

  constructor(private http: HttpClient, private route: ActivatedRoute) {
  }

  ngOnInit() {
    this.http.get<Party[]>(`api/events/${this.getEventAcronym()}/partyOverview`)
      .subscribe(result => { this.parties = result; },
        error => console.error(error));
  }

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }
}

class Party {
  name: string;
  total: number;
  potential: number;
  direct: number;
  id: string;
}
