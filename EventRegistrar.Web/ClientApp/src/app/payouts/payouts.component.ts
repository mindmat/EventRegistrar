import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'payouts',
  templateUrl: './payouts.component.html'
})
export class PayoutsComponent {
  payoutsPending: Payout[];
  payoutsDone: Payout[];

  constructor(http: HttpClient, private route: ActivatedRoute) {
    http.get<Payout[]>(`api/events/${this.getEventAcronym()}/payouts`).subscribe(result => {
      this.payoutsPending = result.filter(por => por.state !== 3);
      this.payoutsDone = result.filter(por => por.state === 3);
    },
      error => console.error(error));
  }

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }
}

class Payout {
  registrationId: string;
  firstName: string;
  lastName: string;
  price: number;
  paid: number;
  created: Date;
  reason: string;
  payments: Payment[];
  stateText: string;
  state: number;
}

class Payment {
  assigned: number;
  paymentAmount: number;
  paymentBookingDate: Date;
  paymentDebitorIban: string;
  paymentDebitorName: string;
  paymentMessage: string;
  paymentInfo: string;
}
