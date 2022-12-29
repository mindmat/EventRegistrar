import { Injectable } from '@angular/core';
import { Resolve, RouterStateSnapshot, ActivatedRouteSnapshot, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { BulkMailTemplatesService } from './bulk-mail-templates.service';

@Injectable({
  providedIn: 'root'
})
export class BulkMailTemplatesResolver implements Resolve<boolean>
{
  constructor(private router: Router, private service: BulkMailTemplatesService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
  {
    return this.service.fetchBulkMailTemplates();
  }
}
