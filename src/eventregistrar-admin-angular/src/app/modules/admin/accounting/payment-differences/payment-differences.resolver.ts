import { Injectable } from '@angular/core';
import { RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { PaymentDifferencesService } from './payment-differences.service';

@Injectable({
  providedIn: 'root'
})
export class PaymentDifferencesResolver 
{
  constructor(private service: PaymentDifferencesService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
  {
    return this.service.fetchDifferences();
  }
}
