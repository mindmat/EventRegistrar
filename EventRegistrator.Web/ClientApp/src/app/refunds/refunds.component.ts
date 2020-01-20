import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'refunds',
  templateUrl: './refunds.component.html'
})
export class RefundsComponent {
  refunds: Refund[];

  constructor(http: HttpClient, private route: ActivatedRoute) {
    http.get<Refund[]>(`api/events/${this.getEventAcronym()}/refunds`).subscribe(result => {
      this.refunds = result;
    },
      error => console.error(error));
  }

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }
}

class Refund {
  registrationId: string;
  firstName: string;
  lastName: string;
  price: number;
  paid: number;
  refundPercentage: number;
  refund: number;
  cancellationDate: Date;
  cancellationReason: string;
}
