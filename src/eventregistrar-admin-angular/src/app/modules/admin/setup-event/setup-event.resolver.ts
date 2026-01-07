import { Injectable } from '@angular/core';
import { RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { SetupEventService } from './setup-event.service';

@Injectable({
  providedIn: 'root'
})
export class SetupEventResolver 
{
  constructor(private service: SetupEventService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
  {
    return this.service.fetchState();
  }
}
