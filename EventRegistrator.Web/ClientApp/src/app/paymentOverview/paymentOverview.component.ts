import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'paymentOverview',
  templateUrl: './paymentOverview.component.html'
})
export class PaymentOverviewComponent {
  public paymentOverview: PaymentOverview;
  public showPotentialDetails: boolean;

  constructor(http: HttpClient, private route: ActivatedRoute) {
    http.get<PaymentOverview>(`api/events/${this.getEventAcronym()}/payments/overview`).subscribe(result => {
      this.paymentOverview = result;
      this.paymentOverview.PotentialOfOpenSpotsSum = this.addOpenSpots(this.paymentOverview.PotentialOfOpenSpots);
    }, error => console.error(error));
  }

  private addOpenSpots(openSpots: OpenSpots[]) {
    return openSpots.map(spot => spot.PotentialIncome).reduce((sum, currrent) => sum + currrent);
  }

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }
}

interface PaymentOverview {
  Balance: Balance;
  ReceivedMoney: number;
  PaidRegistrations: number;
  OutstandingAmount: number;
  NotFullyPaidRegistrations: number;
  PotentialOfOpenSpotsSum: number;
  PotentialOfOpenSpots: OpenSpots[];
}

interface Balance {
  Balance: number;
  Currency: string;
  AccountIban: string;
  Date: Date;
}
interface OpenSpots {
  RegistrableId: string;
  Name: string;
  SpotsAvailable: number;
  PotentialIncome: number;
}
