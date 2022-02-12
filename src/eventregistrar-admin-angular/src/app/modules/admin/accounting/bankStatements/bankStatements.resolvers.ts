import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, Router, RouterStateSnapshot } from '@angular/router';
import { catchError, merge, Observable, throwError } from 'rxjs';
import { BankStatementsService } from './bankStatements.service';


@Injectable({
    providedIn: 'root'
})
export class BankStatementsResolver implements Resolve<any>
{
    constructor(private router: Router, private bankStatementsService: BankStatementsService)
    {
    }

    resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
    {
        return this.bankStatementsService.fetchAllBankStatements()
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
                    return throwError(error);
                })
            );
    }
}
