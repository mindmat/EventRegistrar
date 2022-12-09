import { Injectable } from '@angular/core';
import { Resolve, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { MatchPartnerService } from './match-partner.service';

@Injectable({ providedIn: 'root' })
export class MatchPartnerResolver implements Resolve<boolean>
{
  constructor(private service: MatchPartnerService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean>
  {
    return this.service.fetchCandidates(route.paramMap.get('id'));

  }
}
