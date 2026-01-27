import { Injectable } from '@angular/core';
import { Resolve } from '@angular/router';
import { Observable } from 'rxjs';
import { CalendarViewService } from './calendar-view.service';
import { DateGroup } from 'app/api/api';

@Injectable({
    providedIn: 'root'
})
export class CalendarViewResolver implements Resolve<DateGroup[]>
{
    /**
     * Constructor
     */
    constructor(private _calendarViewService: CalendarViewService)
    {
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    /**
     * Resolver
     *
     * @param route
     * @param state
     */
    resolve(): Observable<DateGroup[]>
    {
        return this._calendarViewService.fetchCalendarData();
    }
}