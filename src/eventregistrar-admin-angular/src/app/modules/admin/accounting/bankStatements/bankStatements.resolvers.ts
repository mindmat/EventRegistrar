import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { BankStatementsService } from './bankStatements.service';


@Injectable({
    providedIn: 'root'
})
export class BankStatementsResolver 
{
    constructor(private router: Router, private bankStatementsService: BankStatementsService)
    {
    }

    resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
    {
        return this.bankStatementsService.fetchBankStatements();
    }
}
