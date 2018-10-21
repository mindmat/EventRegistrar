import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';
import { EventService } from '../events/eventService.service';

@Component({
  selector: 'paymentOverview',
  templateUrl: './paymentOverview.component.html'
})
export class PaymentOverviewComponent {
  public paymentOverview: PaymentOverview;
  public showPotentialDetails: boolean;

  constructor(http: HttpClient, private route: ActivatedRoute, private eventService: EventService) {
    http.get<PaymentOverview>(`api/events/${this.getEventAcronym()}/payments/overview`).subscribe(result => {
      this.paymentOverview = result;
      this.paymentOverview.potentialOfOpenSpotsSum = this.addOpenSpots(this.paymentOverview.potentialOfOpenSpots);
    }, error => console.error(error));
  }

  private addOpenSpots(openSpots: OpenSpots[]) {
    return openSpots.map(spot => spot.potentialIncome).reduce((sum, currrent) => sum + currrent);
  }

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }
}

class PaymentOverview {
  balance: Balance;
  receivedMoney: number;
  paidRegistrations: number;
  outstandingAmount: number;
  notFullyPaidRegistrations: number;
  potentialOfOpenSpotsSum: number;
  potentialOfOpenSpots: OpenSpots[];
}

class Balance {
  balance: number;
  currency: string;
  accountIban: string;
  date: Date;
}
class OpenSpots {
  registrableId: string;
  name: string;
  spotsAvailable: number;
  potentialIncome: number;
}
