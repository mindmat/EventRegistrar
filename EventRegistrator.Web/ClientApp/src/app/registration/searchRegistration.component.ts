import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'searchRegistration',
  templateUrl: './searchRegistration.component.html'
})
export class SearchRegistrationComponent {
  registrations: Registration[];
  isSearching: boolean;

  constructor(private http: HttpClient, private route: ActivatedRoute) {
    this.isSearching = false;
  }

  search(searchString: string) {
    this.isSearching = true;
    this.http.get<Registration[]>(`api/events/${this.getEventAcronym()}/registrations?searchstring=${searchString}`)
      .subscribe(result => {
        this.registrations = result;
        this.registrations.map(reg => reg.responsesJoined = reg.responses.map(rsp => `${rsp.question} = ${rsp.response}`)
          .reduce((agg, line) => `${agg} / ${line}`));
        this.isSearching = false;
      },
        error => console.error(error));
  }

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }
}

class Registration {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  language: string;
  responses: Response[];
  responsesJoined: string;
}

class Response {
  response: string;
  question: string;
}
