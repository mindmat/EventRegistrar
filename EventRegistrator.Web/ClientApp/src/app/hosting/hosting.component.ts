import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'hosting',
  templateUrl: './hosting.component.html'
})
export class HostingComponent {
  public offers: HostingOffer[];
  public seekers: HostingSeeker[];

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    http.get<HostingOffer[]>(baseUrl + 'api/hostingoffers').subscribe(result => {
      this.offers = result;
    }, error => console.error(error));
    http.get<HostingSeeker[]>(baseUrl + 'api/hostingseekers').subscribe(result => {
      this.seekers = result;
    }, error => console.error(error));
  }
}

interface HostingOffer {
  Id: number;
  FirstName: string;
  LastName: string;
  Mail: string;
  Phone: string;
  Language: string;
  State: number;
  Address: string;
  PlaceCount: number;
  Comment: string;
  AdmittedAt: Date;
}

interface HostingSeeker {
  Id: number;
  FirstName: string;
  LastName: string;
  Mail: string;
  Phone: string;
  Language: string;
  State: number;
  Partners: string;
  Travel: string;
  Comment: string;
  AdmittedAt: Date;
}
