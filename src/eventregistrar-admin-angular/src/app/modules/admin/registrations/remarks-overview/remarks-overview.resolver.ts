import { Injectable } from '@angular/core';
import { Resolve, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { RemarksOverviewService } from './remarks-overview.service';

@Injectable({
  providedIn: 'root'
})
export class RemarksOverviewResolver implements Resolve<boolean>
{
  constructor(private remarksOverviewService: RemarksOverviewService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean>
  {
    return this.remarksOverviewService.fetchRemarks();
  }
}
