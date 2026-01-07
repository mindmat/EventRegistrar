import { Injectable } from '@angular/core';
import { RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { AllParticipantsService } from './all-participants.service';

@Injectable({
  providedIn: 'root'
})
export class AllParticipantsResolver 
{
  constructor(private service: AllParticipantsService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
  {
    return this.service.fetchItemsOf('', false);
  }
}
