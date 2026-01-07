import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Observable, zip } from 'rxjs';
import { OverviewService } from './overview.service';
import { PaymentOverviewService } from './payment-overview.service';
import { PricePackagesOverviewService } from './price-packages-overview.service';
import { RegistrationsPerDayService } from './registrations-per-day.service';

@Injectable({
    providedIn: 'root'
})
export class OverviewResolver 
{
    constructor(private overviewService: OverviewService,
        private paymentOverviewService: PaymentOverviewService,
        private pricePackagesOverviewService: PricePackagesOverviewService,
        private registrationsPerDayService: RegistrationsPerDayService) { }

    resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
    {
        return zip(
            this.overviewService.fetchRegistrableTags(),
            this.overviewService.fetchRegistrables(),
            this.paymentOverviewService.fetchData(),
            this.pricePackagesOverviewService.fetchData(),
            this.registrationsPerDayService.fetchData()
        );
    }
}
