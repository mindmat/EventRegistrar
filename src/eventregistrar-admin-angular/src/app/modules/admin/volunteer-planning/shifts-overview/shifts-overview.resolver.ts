import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { VolunteerPlanningService } from '../volunteer-planning.service';

@Injectable({
    providedIn: 'root'
})
export class ShiftsOverviewResolver 
{

    constructor(private _volunteerPlanningService: VolunteerPlanningService) { }

    resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
    {
        return this._volunteerPlanningService.fetchShifts().pipe(
            map(shifts => ({ shifts }))
        );
    }
}
