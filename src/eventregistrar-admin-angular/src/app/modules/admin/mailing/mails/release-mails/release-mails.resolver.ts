import { Injectable } from '@angular/core';
import
{ Router, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { catchError, Observable, of, throwError } from 'rxjs';
import { ReleaseMailsService } from './release-mails.service';

@Injectable({
  providedIn: 'root'
})
export class ReleaseMailsResolver 
{
  constructor(private router: Router, private service: ReleaseMailsService)
  {
  }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
  {
    return this.service.fetchPendingMails()
      .pipe(
        // Error here means the requested task is not available
        catchError((error) =>
        {
          // Log the error
          console.error(error);

          // Get the parent url
          const parentUrl = state.url.split('/').slice(0, -1).join('/');

          // Navigate to there
          this.router.navigateByUrl(parentUrl);

          // Throw an error
          return throwError(() => error);
        })
      );
  }
}
