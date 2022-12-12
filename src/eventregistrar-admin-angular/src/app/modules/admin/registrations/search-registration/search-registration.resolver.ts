import { Injectable } from '@angular/core';
import { Resolve, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { Observable, zip } from 'rxjs';
import { UnprocessedRawRegistrationsService } from '../unprocessed-raw-registrations.service';
import { SearchRegistrationService } from './search-registration.service';

@Injectable({
  providedIn: 'root'
})
export class SearchRegistrationResolver implements Resolve<boolean> {
  constructor(private service: SearchRegistrationService,
    private unprocessedRawRegistrationsService: UnprocessedRawRegistrationsService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
  {
    return zip(this.service.fetchItemsOf(''), this.unprocessedRawRegistrationsService.fetchInfo());
  }
}
