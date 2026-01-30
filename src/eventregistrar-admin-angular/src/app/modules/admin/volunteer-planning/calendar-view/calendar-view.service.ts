import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Api, DateGroup, TracksCalendarQuery } from 'app/api/api';
import { EventService } from '../../events/event.service';
import { FetchService } from '../../infrastructure/fetchService';
import { NotificationService } from '../../infrastructure/notification.service';

@Injectable({
    providedIn: 'root'
})
export class CalendarViewService extends FetchService<DateGroup[]>
{
    /**
     * Constructor
     */
    constructor(
        private _api: Api,
        private _eventService: EventService,
        notificationService: NotificationService
    )
    {
        super('TracksCalendarQuery', notificationService);
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Accessors
    // -----------------------------------------------------------------------------------------------------

    /**
     * Getter for calendar data
     */
    get calendarData$(): Observable<DateGroup[]>
    {
        return this.result$;
    }

    /**
     * Getter for current calendar data value
     */
    get calendarData(): DateGroup[]
    {
        return this.current || [];
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    /**
     * Fetch calendar data from API
     */
    fetchCalendarData(): Observable<DateGroup[]>
    {
        const query: TracksCalendarQuery = {
            eventId: this._eventService.selectedId
        };

        const request = this._api.tracksCalendar_Query(query);

        return this.fetchItems(request, null, this._eventService.selectedId);
    }

    /**
     * Reset calendar data
     */
    resetCalendarData(): void
    {
        // The FetchService base class handles the internal state
        // We can trigger a refresh if needed, or the NotificationService will handle updates
        this.refresh();
    }
}
