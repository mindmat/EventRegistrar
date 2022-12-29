import { Injectable } from '@angular/core';
import { Router, Resolve, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { BulkMailTemplateService } from './bulk-mail-template.service';

@Injectable({
  providedIn: 'root'
})
export class BulkMailTemplateResolver implements Resolve<boolean>
{
  constructor(private router: Router, private service: BulkMailTemplateService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
  {
    return this.service.fetchTemplate(route.paramMap.get('id'));
  }
}
