import { Injectable } from '@angular/core';
import { Resolve, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { Observable, of, zip } from 'rxjs';
import { FormsService } from './forms.service';
import { RegistrablesService } from './registrables.service';

@Injectable({
  providedIn: 'root'
})
export class FormMappingResolver implements Resolve<boolean>
{
  constructor(private formsService: FormsService,
    private registrablesService: RegistrablesService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
  {
    return zip(this.formsService.fetchForms(), this.registrablesService.fetchRegistrables());
  }
}
