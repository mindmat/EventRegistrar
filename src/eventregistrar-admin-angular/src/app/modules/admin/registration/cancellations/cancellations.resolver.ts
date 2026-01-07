import { Injectable } from '@angular/core';
import { RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { CancellationsService } from './cancellations.service';

@Injectable({
  providedIn: 'root'
})
export class CancellationsResolver 
{
  constructor(private service: CancellationsService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
  {
    return this.service.fetchCancellations();
  }
}
