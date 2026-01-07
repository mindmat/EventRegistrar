import { Injectable } from '@angular/core';
import { RouterStateSnapshot, ActivatedRouteSnapshot, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { BulkMailTemplatesService } from './bulk-mail-templates.service';

@Injectable({
  providedIn: 'root'
})
export class BulkMailTemplatesResolver 
{
  constructor(private router: Router, private service: BulkMailTemplatesService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
  {
    return this.service.fetchBulkMailTemplates();
  }
}
