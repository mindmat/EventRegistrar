import { Injectable } from '@angular/core';
import { RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { DuePaymentsService } from './due-payments.service';

@Injectable({
  providedIn: 'root'
})
export class DuePaymentsResolver 
{
  constructor(private duePaymentService: DuePaymentsService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
  {
    return this.duePaymentService.fetchDuePayments();
  }
}
