import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'hosting',
  templateUrl: './hosting.component.html'
})
export class HostingComponent {
  offers: HostingOffers;
  seekers: HostingSeeker[];

  constructor(http: HttpClient, private readonly route: ActivatedRoute) {
    http.get<HostingOffers>(`api/events/${this.getEventAcronym()}/hosting/offers`).subscribe(result => {
      this.offers = result;
    },
      error => console.error(error));
    http.get<HostingSeeker[]>(`api/events/${this.getEventAcronym()}/hosting/requests`).subscribe(result => {
      this.seekers = result;
    },
      error => console.error(error));
  }

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }
}

class HostingOffers {
  dynamicColumns: string[];
  offers: HostingOffer[];
}

class HostingOffer {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  language: string;
  state: number;
  admittedAt: Date;
  columns: IDictionary;
}

interface IDictionary {
  [index: string]: string;
}

class HostingSeeker {
  id: number;
  firstName: string;
  lastName: string;
  mail: string;
  phone: string;
  language: string;
  state: number;
  admittedAt: Date;
  columns: IDictionary;
}
