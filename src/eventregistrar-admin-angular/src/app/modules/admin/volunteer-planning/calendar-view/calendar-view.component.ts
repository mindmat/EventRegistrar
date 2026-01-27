import { Component, OnDestroy, OnInit } from '@angular/core';
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

    private _unsubscribeAll: Subject<any> = new Subject<any>();

    constructor(
        private _calendarViewService: CalendarViewService
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
                console.log('CalendarViewComponent: calendarData updated', data);
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

        this.calendarData.forEach(dateGroup =>
        {
            if (dateGroup.locationGroups)
            {
                dateGroup.locationGroups.forEach(locationGroup =>
                {
                    if (locationGroup.items)
                    {
                        locationGroup.items.forEach(item =>
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

        this.calendarData.forEach(dateGroup =>
        {
            if (dateGroup.locationGroups)
            {
                dateGroup.locationGroups.forEach(locationGroup =>
                {
                    const dateLocationRow: CalendarRow = {
                        date: dateGroup.date!,
                        location: locationGroup.location || 'No Location',
                        dateLocationKey: this.getDateLocationKey(dateGroup.date!, locationGroup.location),
                        displayDate: this.formatDate(dateGroup.date!),
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
            if (dateCompare !== 0) return dateCompare;
            return a.location.localeCompare(b.location);
        });

        // Build cells for each row (hour cells)
        this.rows = dateLocationCombinations.map(row =>
        {
            const cells: CalendarCell[] = this.hours.map(hour =>
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

        this.calendarData.forEach(dateGroup =>
        {
            if (dateGroup.locationGroups)
            {
                dateGroup.locationGroups.forEach(locationGroup =>
                {
                    const currentKey = this.getDateLocationKey(dateGroup.date!, locationGroup.location);

                    if (currentKey === dateLocationKey && locationGroup.items)
                    {
                        locationGroup.items.forEach(item =>
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
     * Format date for display
     */
    private formatDate(date: Date): string
    {
        return new Date(date).toLocaleDateString('en-US', {
            weekday: 'short',
            month: 'short',
            day: 'numeric'
        });
    }

    /**
     * Format hour for display
     */
    private formatHour(hour: number): string
    {
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
}