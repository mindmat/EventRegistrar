import { Injectable } from '@angular/core';
import { Resolve, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { HostingOverviewService } from './hosting-overview.service';

@Injectable({
  providedIn: 'root'
})
export class HostingOverviewResolver implements Resolve<any> {
  constructor(private overviewService: HostingOverviewService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
  {
    return this.overviewService.fetchHosting();
  }
}
