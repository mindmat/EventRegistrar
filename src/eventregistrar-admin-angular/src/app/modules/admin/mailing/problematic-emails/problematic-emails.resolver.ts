import { Injectable } from '@angular/core';
import { Resolve, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { Observable, of, zip } from 'rxjs';
import { ProblematicEmailsService } from './problematic-emails.service';
import { MailDeliverySuccessService } from './mail-delivery-success.service';

@Injectable({
  providedIn: 'root'
})
export class ProblematicEmailsResolver implements Resolve<any>
{
  constructor(private service: ProblematicEmailsService,
    private deliveryService: MailDeliverySuccessService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
  {
    return zip(this.service.fetchItems(), this.deliveryService.fetchStats());
  }
}
