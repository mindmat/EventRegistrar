import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { VolunteerPlanningService } from '../volunteer-planning.service';

@Injectable({
    providedIn: 'root'
})
export class ShiftEditResolver implements Resolve<any>
{

    constructor(private _volunteerPlanningService: VolunteerPlanningService) { }

    resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
    {
        // If we have an ID parameter, we're editing an existing shift
        const shiftId = route.paramMap.get('id');

        if (shiftId && shiftId !== 'new')
        {
            // Get all shifts and find the specific one
            // In a real implementation, you'd have a specific query for single shift
            return this._volunteerPlanningService.fetchShifts().pipe(
                map((shifts: any[]) =>
                {
                    const shift = shifts.find(s => s.id === shiftId);
                    return { shift };
                })
            );
        }

        // If no ID or 'new', we're creating a new shift
        return of({ shift: null });
    }
}