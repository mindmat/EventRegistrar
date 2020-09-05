import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-invalid-email-addresses',
  templateUrl: './invalid-email-addresses.component.html',
  styleUrls: ['./invalid-email-addresses.component.css']
})
export class InvalidEmailAddressesComponent implements OnInit {
  addresses: InvalidEmailAddress[];
  constructor(private readonly http: HttpClient, private readonly route: ActivatedRoute) { }

  ngOnInit() {
    this.http.get<InvalidEmailAddress[]>(`api/events/${this.getEventAcronym()}/invalidMailAddresses`)
      .subscribe(result => {
        this.addresses = result;
      },
        error => console.error(error));
  }

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }

  correctMailAddress(address: InvalidEmailAddress) {
    address.locked = true;

    this.http.post(`api/events/${this.getEventAcronym()}/registrations/${address.registrationId}/fixEmailAddress?oldEmailAddress=${address.email}&newEmailAddress=${address.correction}`, null)
      .subscribe(result => {
      },
        error => {
          console.error(error);
          address.locked = false;
        });
  }
}

class InvalidEmailAddress {
  registrationId: string;
  registrationState: string;
  firstName: string;
  lastName: string;
  email: string;
  lastMailSent: Date;
  lastMailState: string;
  proposals: string;
  correction: string;
  locked: boolean;
}
