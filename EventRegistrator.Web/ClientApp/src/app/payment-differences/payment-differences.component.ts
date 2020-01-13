import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'payment-differences',
  templateUrl: './payment-differences.component.html'
})
export class PaymentDifferencesComponent {
  differences: Difference[];

  constructor(http: HttpClient, private route: ActivatedRoute) {
    http.get<Difference[]>(`api/events/${this.getEventAcronym()}/differences`).subscribe(result => {
      this.differences = result;
    },
      error => console.error(error));
  }

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }
}

class Difference {
  registrationId: string;
  price: number;
  amountPaid: number;
  difference: number;
  firstName: string;
  lastName: string;
}
