import { Injectable } from '@angular/core';
import
{
  Router, Resolve,
  RouterStateSnapshot,
  ActivatedRouteSnapshot
} from '@angular/router';
import { Observable, of } from 'rxjs';
import { MailViewerService } from './mail-viewer.service';

@Injectable({
  providedIn: 'root'
})
export class MailViewerResolver implements Resolve<boolean>
{
  constructor(private router: Router, private service: MailViewerService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
  {
    return this.service.fetchMail(route.paramMap.get('mailId'));
  }
}
