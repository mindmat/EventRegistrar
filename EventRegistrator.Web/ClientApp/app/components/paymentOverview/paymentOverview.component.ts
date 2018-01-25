import { Component, Inject } from '@angular/core';
import { Http } from '@angular/http';

@Component({
    selector: 'paymentOverview',
    templateUrl: './paymentOverview.component.html'
})
export class PaymentOverviewComponent {
    public paymentOverview: PaymentOverview;
    public showPotentialDetails: boolean;

    constructor(http: Http, @Inject('BASE_URL') baseUrl: string) {
        var eventId = '762A93A4-56E0-402C-B700-1CFB3362B39D';
        http.get(`${baseUrl}api/event/${eventId}/PaymentOverview`).subscribe(result => {
            this.paymentOverview = result.json() as PaymentOverview;
            this.paymentOverview.PotentialOfOpenSpotsSum = this.addOpenSpots(this.paymentOverview.PotentialOfOpenSpots);
        }, error => console.error(error));
    }

    private addOpenSpots(openSpots: OpenSpots[]) {
        return openSpots.map(spot => spot.PotentialIncome).reduce((sum, currrent) => sum + currrent);
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