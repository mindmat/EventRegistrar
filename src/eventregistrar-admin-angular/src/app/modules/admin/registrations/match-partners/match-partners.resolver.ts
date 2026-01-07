import { Injectable } from '@angular/core';
import { Router, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { MatchPartnersService } from './match-partners.service';

@Injectable({ providedIn: 'root' })
export class MatchPartnersResolver 
{
  constructor(private router: Router, private service: MatchPartnersService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
  {
    return this.service.fetchUnmatchedPartners();
  }
}
