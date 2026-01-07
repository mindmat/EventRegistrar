import { Injectable } from '@angular/core';
import { RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { PayoutsService } from './payouts.service';

@Injectable({
  providedIn: 'root'
})
export class PayoutsResolver 
{
  constructor(private service: PayoutsService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
  {
    return this.service.fetchPayouts();
  }
}
