import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { merge, Observable, zip } from 'rxjs';
import { OverviewService } from './overview.service';

@Injectable({
    providedIn: 'root'
})
export class OverviewResolver implements Resolve<any>
{
    constructor(private overviewService: OverviewService)
    {
    }

    resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
    {
        return zip(
            this.overviewService.fetchRegistrableTags(),
            this.overviewService.fetchSingleRegistrables(),
            this.overviewService.fetchDoubleRegistrables());
    }
}
