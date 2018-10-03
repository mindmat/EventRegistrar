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
    this.http.get<Registration[]>(`api/events/${this.getEventAcronym()}/registrations?searchstring=${searchString}&states=received&states=paid&states=cancelled`)
      .subscribe(result => {
        this.registrations = result;
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
  price: number;
  amountPaid: number;
  state: number;
  stateText: string;
}
