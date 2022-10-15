import { Injectable } from '@angular/core';
import { Resolve, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { Observable, of } from 'rxjs';
import { FormsService } from './forms.service';

@Injectable({
  providedIn: 'root'
})
export class FormMappingResolver implements Resolve<boolean>
{
  constructor(private formsService: FormsService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean>
  {
    return this.formsService.fetchForms();
  }
}
