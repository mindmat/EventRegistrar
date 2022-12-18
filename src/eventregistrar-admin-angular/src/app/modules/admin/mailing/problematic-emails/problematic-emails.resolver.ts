import { Injectable } from '@angular/core';
import { Resolve, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { Observable, of } from 'rxjs';
import { ProblematicEmailsService } from './problematic-emails.service';

@Injectable({
  providedIn: 'root'
})
export class ProblematicEmailsResolver implements Resolve<any>
{
  constructor(private service: ProblematicEmailsService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
  {
    return this.service.fetchItems();
  }
}
