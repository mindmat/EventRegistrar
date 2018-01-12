import { Component, Inject } from '@angular/core';
import { Http } from '@angular/http';

@Component({
  selector: 'hosting',
  templateUrl: './hosting.component.html'
})
export class HostingComponent {
  public offers: HostingOffer[];
  public seekers: HostingSeeker[];

  constructor(http: Http, @Inject('BASE_URL') baseUrl: string) {
    http.get(baseUrl + 'api/hostingoffers').subscribe(result => {
        this.offers = result.json() as HostingOffer[];
    }, error => console.error(error));
    http.get(baseUrl + 'api/hostingseekers').subscribe(result => {
        this.seekers = result.json() as HostingSeeker[];
    }, error => console.error(error));
  }
}

interface HostingOffer {
    Id: number;
    FirstName: string;
    LastName: string;
    Mail: string;
    Phone: string;
    State: number;
    Address: string;
    PlaceCount: number;
    Comment: string;
}

interface HostingSeeker {
    Id: number;
    FirstName: string;
    LastName: string;
    Mail: string;
    Phone: string;
    State: number;
    Partners: string;
    Travel: string;
    Comment: string;
}