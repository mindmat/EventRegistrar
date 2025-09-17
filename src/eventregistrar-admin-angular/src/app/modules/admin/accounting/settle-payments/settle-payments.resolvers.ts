import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, Router, RouterStateSnapshot } from '@angular/router';
import { catchError, Observable, of, tap } from 'rxjs';
import { SettlePaymentsService } from './settle-payments.service';
import { BookingsOfDay } from 'app/api/api';


@Injectable({
    providedIn: 'root'
})
export class SettlePaymentsResolver implements Resolve<any>
{
    private initialData: BookingsOfDay[] | null = null;
    constructor(private router: Router, private statementsService: SettlePaymentsService)
    {
    }

    resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
    {
        if (this.initialData)
        {
            return of(this.initialData);
        }

        return this.statementsService.fetchBankStatements()
            .pipe(
                tap((data) =>
                {
                    this.initialData = data;
                }),
                catchError((error) =>
                {
                    console.error('Error fetching bank statements:', error);
                    return of([]);
                })
            );
    }
}
