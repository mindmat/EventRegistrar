import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'searchRegistration',
  templateUrl: './searchRegistration.component.html'
})
export class SearchRegistrationComponent {
  registrations: Registration[];
  isSearching: boolean;
  searchString: string;

  constructor(private http: HttpClient, private router: Router, private route: ActivatedRoute) {
    this.isSearching = false;
    var searchParam = route.snapshot.queryParams['search'];
    if (searchParam != null) {
      this.searchString = searchParam;
      this.search();
    }
  }

  search() {
    this.isSearching = true;
    this.router.navigate(
      [],
      {
        relativeTo: this.route,
        queryParams: { search: this.searchString },
        queryParamsHandling: 'merge'
      });
    this.http.get<Registration[]>(`api/events/${this.getEventAcronym()}/registrations?searchstring=${this.searchString}&states=received&states=paid&states=cancelled`)
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
