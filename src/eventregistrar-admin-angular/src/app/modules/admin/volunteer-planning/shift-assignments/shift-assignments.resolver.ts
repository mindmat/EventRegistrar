import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Observable, forkJoin, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { VolunteerPlanningService } from '../volunteer-planning.service';

@Injectable({
    providedIn: 'root'
})
export class ShiftAssignmentsResolver implements Resolve<any>
{

    constructor(private _volunteerPlanningService: VolunteerPlanningService) { }

    resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
    {
        const shiftId = route.paramMap.get('id');

        if (!shiftId)
        {
            return of(null);
        }

        return forkJoin({
            shifts: this._volunteerPlanningService.fetchShifts(),
            availableParticipants: this._volunteerPlanningService.getAvailableParticipants(shiftId)
        }).pipe(
            map(result =>
            {
                const shift = result.shifts.find(s => s.id === shiftId);
                return {
                    shift: shift,
                    availableParticipants: result.availableParticipants,
                    assignedParticipants: shift?.assignedParticipants || []
                };
            })
        );
    }
}