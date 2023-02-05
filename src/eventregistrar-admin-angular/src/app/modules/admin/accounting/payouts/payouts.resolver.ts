import { Injectable } from '@angular/core';
import { Resolve, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { PayoutsService } from './payouts.service';

@Injectable({
  providedIn: 'root'
})
export class PayoutsResolver implements Resolve<any>
{
  constructor(private service: PayoutsService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
  {
    return this.service.fetchPayouts();
  }
}
