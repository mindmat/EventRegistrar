import { Component } from '@angular/core';
import { Http } from '@angular/http';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'searchRegistration',
  templateUrl: './searchRegistration.component.html'
})
export class SearchRegistrationComponent {
  registrations: Registration[];
  isSearching: boolean;

  constructor(private readonly http: Http, private route: ActivatedRoute) {
    this.isSearching = false;
  }

  search(searchString: string) {
    this.isSearching = true;
    this.http.get(`api/events/${this.getEventAcronym()}/registrations?searchstring=${searchString}`)
      .subscribe(result => {
        this.registrations = result.json() as Registration[];
        this.registrations.map(reg => reg.ResponsesJoined = reg.Responses.map(rsp => `${rsp.Question} = ${rsp.Response}`)
          .reduce((agg, line) => `${agg} / ${line}`));
        this.isSearching = false;
      },
        error => console.error(error));
  }

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }
}

interface Registration {
  Id: string;
  Email: string;
  FirstName: string;
  LastName: string;
  Language: string;
  Responses: Response[];
  ResponsesJoined: string;
}

interface Response {
  Response: string;
  Question: string;
}
