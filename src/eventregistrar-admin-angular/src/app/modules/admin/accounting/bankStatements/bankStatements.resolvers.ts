import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, Router, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
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
        return this.bankStatementsService.fetchBankStatements();
    }
}
