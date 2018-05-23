import { Component, Inject, } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, ActivatedRoute } from '@angular/router';

@Component({
  selector: 'partyOverview',
  templateUrl: './partyOverview.component.html'
})
export class PartyOverviewComponent {
  parties: Party[];

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private router: Router, private route: ActivatedRoute) {
  }

  ngOnInit() {
    var eventId = '762A93A4-56E0-402C-B700-1CFB3362B39D';
    this.http.get<Party[]>(`${this.baseUrl}api/events/${eventId}/partyOverview`)
      .subscribe(result => { this.parties = result; },
        error => console.error(error));
  }
}

interface Party {
  Id: string;
  Email: string;
  FirstName: string;
  LastName: string;
  Kurs: string;
  MittagessenSamstag: string;
  MittagessenSonntag: string;
  PartyPass: boolean;
  Status: string;
}
