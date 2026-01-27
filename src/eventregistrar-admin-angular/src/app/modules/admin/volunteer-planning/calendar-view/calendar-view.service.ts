import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { Api, DateGroup, RegistrableIcsCalendarViewQuery } from 'app/api/api';
import { EventService } from '../../events/event.service';

@Injectable({
    providedIn: 'root'
})
export class CalendarViewService
{
    private _calendarData: BehaviorSubject<DateGroup[]> = new BehaviorSubject<DateGroup[]>([]);

    /**
     * Constructor
     */
    constructor(
        private _api: Api,
        private _eventService: EventService
    )
    {
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Accessors
    // -----------------------------------------------------------------------------------------------------

    /**
     * Getter for calendar data
     */
    get calendarData$(): Observable<DateGroup[]>
    {
        return this._calendarData.asObservable();
    }

    /**
     * Getter for current calendar data value
     */
    get calendarData(): DateGroup[]
    {
        return this._calendarData.value;
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    /**
     * Fetch calendar data from API
     */
    fetchCalendarData(): Observable<DateGroup[]>
    {
        console.log('CalendarViewService: fetchCalendarData called');
        console.log('CalendarViewService: selectedId:', this._eventService.selectedId);

        const query: RegistrableIcsCalendarViewQuery = {
            eventId: this._eventService.selectedId
        };

        console.log('CalendarViewService: Query object:', query);

        const request = this._api.registrableIcsCalendarView_Query(query);

        // Update the subject with the new data
        request.subscribe({
            next: (data: DateGroup[]) =>
            {
                console.log('CalendarViewService: Received API data:', data);
                this._calendarData.next(data || []);
            },
            error: (error) =>
            {
                console.error('CalendarViewService: API error:', error);
                this._calendarData.next([]);
            }
        });

        return request;
    }

    /**
     * Reset calendar data
     */
    resetCalendarData(): void
    {
        this._calendarData.next([]);
    }
}
