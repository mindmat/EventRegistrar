import { Injectable } from '@angular/core';
import { Resolve, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { PaymentDifferencesService } from './payment-differences.service';

@Injectable({
  providedIn: 'root'
})
export class PaymentDifferencesResolver implements Resolve<any>
{
  constructor(private service: PaymentDifferencesService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
  {
    return this.service.fetchDifferences();
  }
}
