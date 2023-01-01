import { Injectable } from '@angular/core';
import { Router, Resolve, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { Observable, zip } from 'rxjs';
import { RegistrablesService } from '../../pricing/registrables.service';
import { BulkMailTemplateService } from './bulk-mail-template.service';
import { GeneratedBulkMailsService } from './generated-bulk-mails.service';

@Injectable({
  providedIn: 'root'
})
export class BulkMailTemplateResolver implements Resolve<boolean>
{
  constructor(private router: Router,
    private service: BulkMailTemplateService,
    private registrablesService: RegistrablesService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
  {
    return zip(
      this.service.fetchTemplate(route.paramMap.get('id')),
      this.registrablesService.fetchRegistrables());
  }
}
