import { Injectable } from '@angular/core';
import { Resolve, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { Observable, zip } from 'rxjs';
import { PricePackagePartSelectionTypeService } from './pricing-selection-type.service';
import { PricingService } from './pricing.service';
import { RegistrablesService } from './registrables.service';

@Injectable({ providedIn: 'root' })
export class PricingResolver implements Resolve<boolean>
{
  constructor(private pricingService: PricingService,
    private registrablesService: RegistrablesService,
    private pricePackagePartSelectionTypeService: PricePackagePartSelectionTypeService) { }


  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
  {
    return zip(this.pricingService.fetchPricing(),
      this.registrablesService.fetchRegistrables(),
      this.pricePackagePartSelectionTypeService.fetchSelectionTypes()
    );
  }
}
