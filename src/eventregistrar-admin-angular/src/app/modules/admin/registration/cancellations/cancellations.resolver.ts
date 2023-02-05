import { Injectable } from '@angular/core';
import { Resolve, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { CancellationsService } from './cancellations.service';

@Injectable({
  providedIn: 'root'
})
export class CancellationsResolver implements Resolve<any>
{
  constructor(private service: CancellationsService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
  {
    return this.service.fetchCancellations();
  }
}
