import { Injectable } from '@angular/core';
import { Resolve, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { AllParticipantsService } from './all-participants.service';

@Injectable({
  providedIn: 'root'
})
export class AllParticipantsResolver implements Resolve<boolean> {
  constructor(private service: AllParticipantsService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
  {
    return this.service.fetchItemsOf('');
  }
}
