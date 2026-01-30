import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { RegistrableIcsComponent } from '../../registrables/registrable-ics/registrable-ics.component';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { CalendarViewService } from './calendar-view.service';
import { DateGroup, LocationGroup, CalendarIcsItem } from 'app/api/api';

interface CalendarColumn
{
    hour: number;
    displayHour: string;
}

interface CalendarRow
{
    date: Date;
    location: string;
    dateLocationKey: string;
    displayDate: string;
    displayLocation: string;
    cells: CalendarCell[];
}

interface CalendarCell
{
    hour: number;
    items: CalendarIcsItem[];
    isEmpty: boolean;
}

interface CalendarEventItem extends CalendarIcsItem
{
    startHour: number;
    endHour: number;
    duration: number;
    position: number;
}

@Component({
    selector: 'calendar-view',
    templateUrl: './calendar-view.component.html',
    styleUrls: ['./calendar-view.component.scss']
})
export class CalendarViewComponent implements OnInit, OnDestroy
{
    calendarData: DateGroup[] = [];
    columns: CalendarColumn[] = []; // Now represents hours
    rows: CalendarRow[] = []; // Now represents date/location combinations
    hours: number[] = [];
    dateLocations: CalendarRow[] = []; // Temporary for building

    // New properties for time-based calendar
    selectedDateIndex: number = 0;
    dayStartHour: number = 6; // Calendar starts at 6 AM
    dayEndHour: number = 23; // Calendar ends at 11 PM

    private _unsubscribeAll: Subject<any> = new Subject<any>();

    constructor(
        private _calendarViewService: CalendarViewService,
        private _matDialog: MatDialog,
        private _changeDetectorRef: ChangeDetectorRef
    )
    {
    }

    ngOnInit(): void
    {
        // Subscribe to calendar data changes
        this._calendarViewService.calendarData$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((data: DateGroup[]) =>
            {
                this.calendarData = data;
                this.processCalendarData();
                this._changeDetectorRef.markForCheck();
            });

        // Fetch initial data
        this._calendarViewService.fetchCalendarData()
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe();
    }

    ngOnDestroy(): void
    {
        // Unsubscribe from all subscriptions
        this._unsubscribeAll.next(null);
        this._unsubscribeAll.complete();
    }

    // New methods for time-based calendar

    /**
     * Get the currently selected date group
     */
    getSelectedDateGroup(): DateGroup | null
    {
        return this.calendarData && this.calendarData.length > this.selectedDateIndex
            ? this.calendarData[this.selectedDateIndex]
            : null;
    }

    /**
     * Get the selected date group (no longer moves events between days)
     */
    getSelectedDateGroupWithExtendedHours(): DateGroup | null
    {
        return this.getSelectedDateGroup();
    }

    /**
     * Get total number of events across all dates
     */
    getTotalEvents(): number
    {
        return this.calendarData.reduce((total, dateGroup) =>
            total + (dateGroup.locationGroups?.reduce((locationTotal, locationGroup) =>
                locationTotal + (locationGroup.items?.length || 0)
                , 0) || 0)
            , 0);
    }

    /**
     * Get total number of unique locations across all dates
     */
    getLocationCount(): number
    {
        const uniqueLocations = new Set<string>();
        this.calendarData.forEach((dateGroup) =>
        {
            dateGroup.locationGroups?.forEach((locationGroup) =>
            {
                if (locationGroup.location)
                {
                    uniqueLocations.add(locationGroup.location);
                }
            });
        });
        return uniqueLocations.size;
    }
    /**
     * Get hours to display for the current day (only hours covered by events)
     * Events that start on this day but end past midnight will extend the hours accordingly
     */
    getHoursForDay(): number[]
    {
        const selectedDate = this.getSelectedDateGroup();
        if (!selectedDate || !selectedDate.locationGroups)
        {
            return [];
        }

        let earliestHour = 24; // Start with impossible high value
        let latestHour = -1; // Start with impossible low value (can go up to 27+ for next day)

        // Collect all hours that have events for the current day
        selectedDate.locationGroups.forEach((locationGroup) =>
        {
            locationGroup.items?.forEach((item) =>
            {
                if (item.start && item.end)
                {
                    const startDate = new Date(item.start);
                    const endDate = new Date(item.end);
                    const startHour = startDate.getHours();

                    // Track earliest hour (only from events starting on this day)
                    earliestHour = Math.min(earliestHour, startHour);

                    // Calculate end hour - handle events crossing midnight
                    let effectiveEndHour = endDate.getHours();
                    const startDayStr = startDate.toISOString().split('T')[0];
                    const endDayStr = endDate.toISOString().split('T')[0];

                    // If event ends on a different day, add 24 hours for each day difference
                    if (endDayStr !== startDayStr)
                    {
                        const startDay = new Date(startDayStr);
                        const endDay = new Date(endDayStr);
                        const daysDiff = Math.round((endDay.getTime() - startDay.getTime()) / (1000 * 60 * 60 * 24));
                        effectiveEndHour = endDate.getHours() + (daysDiff * 24);
                    }
                    // Handle edge case: event starts late and ends at hour 0 on "same day" (midnight stored as 00:00)
                    else if (startHour > effectiveEndHour)
                    {
                        // Event clearly crosses midnight (e.g., 20:00 to 00:00)
                        effectiveEndHour += 24;
                    }

                    // If event ends exactly at the hour (0 minutes), don't include that hour
                    if (endDate.getMinutes() === 0 && endDate.getSeconds() === 0)
                    {
                        effectiveEndHour--;
                    }

                    // Track latest hour
                    latestHour = Math.max(latestHour, effectiveEndHour);
                }
            });
        });

        // If no valid hours found, return empty array
        if (earliestHour > latestHour || earliestHour === 24 || latestHour === -1)
        {
            return [];
        }

        // Build continuous range with one hour padding at start and end for visual breathing room
        const paddedStart = Math.max(0, earliestHour - 1);
        const hours: number[] = [];
        for (let hour = paddedStart; hour <= latestHour + 1; hour++)
        {
            hours.push(hour);
        }

        return hours;
    }

    /**
     * Get the left position percentage of an event based on its start time
     */
    getEventLeftPosition(item: CalendarIcsItem): number
    {
        if (!item.start)
        {
            return 0;
        }

        const startDate = new Date(item.start);
        const startHour = this.getEventDisplayHour(item);
        const startMinutes = startDate.getMinutes();
        const hours = this.getHoursForDay();

        if (hours.length === 0)
        {
            return 0;
        }

        const startHourIndex = hours.indexOf(startHour);
        if (startHourIndex === -1)
        {
            // If the exact start hour is not in the array, find the closest one
            const closestHourIndex = hours.findIndex(hour => hour > startHour);
            if (closestHourIndex === -1)
            {
                return 95; // Event starts after all displayed hours
            }
            return (closestHourIndex / hours.length) * 100;
        }

        // Calculate position including minutes within the hour slot
        const positionInHours = startHourIndex + (startMinutes / 60);
        return (positionInHours / hours.length) * 100;
    }

    /**
     * Get the width percentage of an event based on its actual duration
     */
    getEventWidth(item: CalendarIcsItem): number
    {
        if (!item.start || !item.end)
        {
            return 8; // Minimum width
        }

        const startDate = new Date(item.start);
        const endDate = new Date(item.end);
        const hours = this.getHoursForDay();

        if (hours.length === 0)
        {
            return 8;
        }

        // Calculate duration in hours (including fractional hours)
        const durationMs = endDate.getTime() - startDate.getTime();
        const durationHours = durationMs / (1000 * 60 * 60);

        // Total hours displayed on the axis
        const totalHoursDisplayed = hours.length;

        // Calculate width as percentage of total displayed hours
        const widthPercentage = (durationHours / totalHoursDisplayed) * 100;

        // Minimum width of 5% for very short events, maximum of 95% to prevent overflow
        return Math.max(5, Math.min(widthPercentage, 95));
    }

    /**
     * Open the event dialog when clicking on an event
     */
    openEventDialog(item: CalendarIcsItem): void
    {
        this._matDialog.open(RegistrableIcsComponent, {
            autoFocus: true,
            data: {
                icsId: item.id,
                registrableId: item.registrableId,
                name: item.registrableName
            }
        });
    }

    /**
     * Get tooltip text for an event
     */
    getEventTooltip(item: CalendarIcsItem): string
    {
        let name: string;
        if (item.title)
        {
            name = item.title;
        }
        else
        {
            name = item.registrableName || 'Event';
            if (item.registrableNameSecondary)
            {
                name += `\n${item.registrableNameSecondary}`;
            }
        }
        const time = item.start && item.end ?
            `${this.formatTime(item.start)} - ${this.formatTime(item.end)} (${this.getItemDuration(item)})` :
            'Time not specified';
        const content = item.contentHtml ? `\n${item.contentHtml.replace(/<[^>]*>/g, '')}` : '';

        return `${name}\n${time}${content}`;
    }

    /**
     * Track by function for hours
     */
    trackByHour(index: number, hour: number): number
    {
        return hour;
    }

    /**
     * Track by function for location groups
     */
    trackByLocationGroup(index: number, locationGroup: LocationGroup): string
    {
        return locationGroup.location || `no-location-${index}`;
    }

    /**
     * Format date for display
     */
    formatDate(date: Date): string
    {
        return new Date(date).toLocaleDateString('de-CH', {
            weekday: 'long',
            day: 'numeric',
            month: 'long',
            year: 'numeric'
        });
    }

    /**
     * Format hour for display
     */
    formatHour(hour: number): string
    {
        // Handle extended hours (24-27 represent midnight-4am of next day)
        if (hour >= 24)
        {
            const displayHour = hour - 24;
            return `${displayHour.toString().padStart(2, '0')}:00+1`;
        }
        return `${hour.toString().padStart(2, '0')}:00`;
    }

    /**
     * Format time for display
     */
    formatTime(date: Date): string
    {
        return new Date(date).toLocaleTimeString('en-US', {
            hour: '2-digit',
            minute: '2-digit',
            hour12: false
        });
    }

    /**
     * Get CSS classes for calendar item
     */
    getItemClasses(item: CalendarIcsItem): string
    {
        const classes = ['calendar-item'];

        // Add registrable-specific classes if needed
        if (item.registrableId)
        {
            classes.push('has-registrable');
        }

        return classes.join(' ');
    }

    /**
     * Get item display duration
     */
    getItemDuration(item: CalendarIcsItem): string
    {
        if (item.start && item.end)
        {
            const start = new Date(item.start);
            const end = new Date(item.end);
            const durationMinutes = (end.getTime() - start.getTime()) / (1000 * 60);

            if (durationMinutes >= 60)
            {
                const hours = Math.floor(durationMinutes / 60);
                const minutes = durationMinutes % 60;
                return minutes > 0 ? `${hours}h ${minutes}m` : `${hours}h`;
            } else
            {
                return `${durationMinutes}m`;
            }
        }
        return '';
    }

    /**
     * Track by function for columns (now hours)
     */
    trackByColumn(index: number, column: CalendarColumn): number
    {
        return column.hour;
    }

    /**
     * Track by function for rows (now date/location combinations)
     */
    trackByRow(index: number, row: CalendarRow): string
    {
        return row.dateLocationKey;
    }

    /**
     * Track by function for items
     */
    trackByItem(index: number, item: CalendarIcsItem): string
    {
        return item.id || `${index}_${item.registrableId}_${item.start}`;
    }

    /**
     * Get the hour value for positioning (always returns actual start hour since we don't move events)
     */
    private getEventDisplayHour(item: CalendarIcsItem): number
    {
        if (!item.start)
        {
            return 0;
        }

        const startDate = new Date(item.start);
        return startDate.getHours();
    }

    /**
     * Process calendar data and build grid structure
     */
    private processCalendarData(): void
    {
        if (!this.calendarData || this.calendarData.length === 0)
        {
            this.columns = [];
            this.rows = [];
            return;
        }

        this.buildColumns();
        this.buildRows();
    }

    /**
     * Build columns (one per hour) - only for hours with events
     */
    private buildColumns(): void
    {
        // Find only the hours that have actual events
        const hourSet = new Set<number>();

        this.calendarData.forEach((dateGroup) =>
        {
            if (dateGroup.locationGroups)
            {
                dateGroup.locationGroups.forEach((locationGroup) =>
                {
                    if (locationGroup.items)
                    {
                        locationGroup.items.forEach((item) =>
                        {
                            if (item.start && item.end)
                            {
                                const startTime = new Date(item.start);
                                const endTime = new Date(item.end);
                                const startHour = startTime.getHours();
                                const endHour = endTime.getHours();

                                // Add all hours that this event spans
                                for (let hour = startHour; hour <= endHour; hour++)
                                {
                                    // Only add the hour if the event actually occupies it
                                    if (hour === startHour ||
                                        hour < endHour ||
                                        (hour === endHour && endTime.getMinutes() > 0))
                                    {
                                        hourSet.add(hour);
                                    }
                                }
                            }
                        });
                    }
                });
            }
        });

        // If no hours found, default to common work hours
        if (hourSet.size === 0)
        {
            for (let i = 8; i <= 18; i++)
            {
                hourSet.add(i);
            }
        }

        // Only use hours that have events (no padding)
        this.hours = Array.from(hourSet).sort((a, b) => a - b);

        // Build hour columns
        this.columns = this.hours.map(hour => ({
            hour,
            displayHour: this.formatHour(hour)
        }));
    }

    /**
     * Build rows (one per date/location combination)
     */
    private buildRows(): void
    {
        // Build date/location combinations
        const dateLocationCombinations: CalendarRow[] = [];

        this.calendarData.forEach((dateGroup) =>
        {
            if (dateGroup.locationGroups && dateGroup.date)
            {
                dateGroup.locationGroups.forEach((locationGroup) =>
                {
                    const dateLocationRow: CalendarRow = {
                        date: dateGroup.date,
                        location: locationGroup.location || 'No Location',
                        dateLocationKey: this.getDateLocationKey(dateGroup.date, locationGroup.location),
                        displayDate: this.formatDate(dateGroup.date),
                        displayLocation: locationGroup.location || 'No Location',
                        cells: []
                    };
                    dateLocationCombinations.push(dateLocationRow);
                });
            }
        });

        // Sort by date first, then by location
        dateLocationCombinations.sort((a, b) =>
        {
            const dateCompare = new Date(a.date).getTime() - new Date(b.date).getTime();
            if (dateCompare !== 0)
            {
                return dateCompare;
            }
            return a.location.localeCompare(b.location);
        });

        // Build cells for each row (hour cells)
        this.rows = dateLocationCombinations.map((row) =>
        {
            const cells: CalendarCell[] = this.hours.map((hour) =>
            {
                const items = this.getItemsForHourAndDateLocation(hour, row.dateLocationKey);
                return {
                    hour,
                    items,
                    isEmpty: items.length === 0
                };
            });

            return {
                ...row,
                cells
            };
        });
    }

    /**
     * Get items that occur during a specific hour for a specific date/location
     */
    private getItemsForHourAndDateLocation(hour: number, dateLocationKey: string): CalendarIcsItem[]
    {
        const items: CalendarIcsItem[] = [];

        this.calendarData.forEach((dateGroup) =>
        {
            if (dateGroup.locationGroups && dateGroup.date)
            {
                dateGroup.locationGroups.forEach((locationGroup) =>
                {
                    const currentKey = this.getDateLocationKey(dateGroup.date, locationGroup.location);

                    if (currentKey === dateLocationKey && locationGroup.items)
                    {
                        locationGroup.items.forEach((item) =>
                        {
                            if (item.start && item.end)
                            {
                                const startHour = new Date(item.start).getHours();
                                const endHour = new Date(item.end).getHours();

                                // Item occurs during this hour if it starts before or at this hour
                                // and ends after this hour (or at the next hour)
                                if (startHour <= hour && endHour > hour)
                                {
                                    items.push(item);
                                }
                            }
                        });
                    }
                });
            }
        });

        return items;
    }

    /**
     * Generate a unique key for date/location combination
     */
    private getDateLocationKey(date: Date, location: string | null): string
    {
        const dateStr = new Date(date).toISOString().split('T')[0];
        const locationStr = location || 'no-location';
        return `${dateStr}_${locationStr}`;
    }

    /**
     * Get hours array for a specific date group
     */
    getHoursForDayGroup(dateGroup: DateGroup): number[]
    {
        if (!dateGroup || !dateGroup.locationGroups)
        {
            return [];
        }

        let earliestHour = 24;
        let latestHour = -1;

        dateGroup.locationGroups.forEach((locationGroup) =>
        {
            locationGroup.items?.forEach((item) =>
            {
                if (item.start && item.end)
                {
                    const startDate = new Date(item.start);
                    const endDate = new Date(item.end);
                    const startHour = startDate.getHours();

                    earliestHour = Math.min(earliestHour, startHour);

                    let effectiveEndHour = endDate.getHours();
                    const startDayStr = startDate.toISOString().split('T')[0];
                    const endDayStr = endDate.toISOString().split('T')[0];

                    if (endDayStr !== startDayStr)
                    {
                        const startDay = new Date(startDayStr);
                        const endDay = new Date(endDayStr);
                        const daysDiff = Math.round((endDay.getTime() - startDay.getTime()) / (1000 * 60 * 60 * 24));
                        effectiveEndHour = endDate.getHours() + (daysDiff * 24);
                    }
                    else if (startHour > effectiveEndHour)
                    {
                        effectiveEndHour += 24;
                    }

                    if (endDate.getMinutes() === 0 && endDate.getSeconds() === 0)
                    {
                        effectiveEndHour--;
                    }

                    latestHour = Math.max(latestHour, effectiveEndHour);
                }
            });
        });

        if (earliestHour > latestHour || earliestHour === 24 || latestHour === -1)
        {
            return [];
        }

        const paddedStart = Math.max(0, earliestHour - 1);
        const hours: number[] = [];
        for (let hour = paddedStart; hour <= latestHour + 1; hour++)
        {
            hours.push(hour);
        }

        return hours;
    }

    /**
     * Get event left position for a specific day
     */
    getEventLeftPositionForDay(item: CalendarIcsItem, dateGroup: DateGroup): number
    {
        if (!item.start)
        {
            return 0;
        }

        const startDate = new Date(item.start);
        const startHour = startDate.getHours();
        const startMinutes = startDate.getMinutes();
        const hours = this.getHoursForDayGroup(dateGroup);

        if (hours.length === 0)
        {
            return 0;
        }

        const startHourIndex = hours.indexOf(startHour);
        if (startHourIndex === -1)
        {
            const closestHourIndex = hours.findIndex(hour => hour > startHour);
            if (closestHourIndex === -1)
            {
                return 95;
            }
            return (closestHourIndex / hours.length) * 100;
        }

        const positionInHours = startHourIndex + (startMinutes / 60);
        return (positionInHours / hours.length) * 100;
    }

    /**
     * Get event width for a specific day
     */
    getEventWidthForDay(item: CalendarIcsItem, dateGroup: DateGroup): number
    {
        if (!item.start || !item.end)
        {
            return 8;
        }

        const startDate = new Date(item.start);
        const endDate = new Date(item.end);
        const hours = this.getHoursForDayGroup(dateGroup);

        if (hours.length === 0)
        {
            return 8;
        }

        const durationMs = endDate.getTime() - startDate.getTime();
        const durationHours = durationMs / (1000 * 60 * 60);
        const totalHoursDisplayed = hours.length;
        const widthPercentage = (durationHours / totalHoursDisplayed) * 100;

        return Math.max(5, Math.min(widthPercentage, 95));
    }
}

