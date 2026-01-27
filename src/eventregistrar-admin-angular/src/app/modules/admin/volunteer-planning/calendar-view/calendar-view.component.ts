import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { CalendarViewService } from './calendar-view.service';
import { DateGroup, LocationGroup, CalendarIcsItem } from 'app/api/api';

interface CalendarColumn
{
    date: Date;
    location: string;
    dateLocationKey: string;
    displayDate: string;
    displayLocation: string;
}

interface CalendarRow
{
    hour: number;
    displayHour: string;
    cells: CalendarCell[];
}

interface CalendarCell
{
    dateLocationKey: string;
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
    columns: CalendarColumn[] = [];
    rows: CalendarRow[] = [];
    hours: number[] = [];

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
     * Build columns (one per date/location combination)
     */
    private buildColumns(): void
    {
        this.columns = [];

        this.calendarData.forEach(dateGroup =>
        {
            if (dateGroup.locationGroups)
            {
                dateGroup.locationGroups.forEach(locationGroup =>
                {
                    const column: CalendarColumn = {
                        date: dateGroup.date!,
                        location: locationGroup.location || 'No Location',
                        dateLocationKey: this.getDateLocationKey(dateGroup.date!, locationGroup.location),
                        displayDate: this.formatDate(dateGroup.date!),
                        displayLocation: locationGroup.location || 'No Location'
                    };
                    this.columns.push(column);
                });
            }
        });

        // Sort columns by date first, then by location
        this.columns.sort((a, b) =>
        {
            const dateCompare = new Date(a.date).getTime() - new Date(b.date).getTime();
            if (dateCompare !== 0) return dateCompare;
            return a.location.localeCompare(b.location);
        });
    }

    /**
     * Build rows (one per hour)
     */
    private buildRows(): void
    {
        // Find the range of hours we need to display
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
                            if (item.start)
                            {
                                const startHour = new Date(item.start).getHours();
                                hourSet.add(startHour);
                            }
                            if (item.end)
                            {
                                const endHour = new Date(item.end).getHours();
                                hourSet.add(endHour);
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
        } else
        {
            // Add padding hours
            const minHour = Math.min(...hourSet);
            const maxHour = Math.max(...hourSet);
            for (let i = Math.max(0, minHour - 1); i <= Math.min(23, maxHour + 1); i++)
            {
                hourSet.add(i);
            }
        }

        this.hours = Array.from(hourSet).sort((a, b) => a - b);

        // Build rows
        this.rows = this.hours.map(hour =>
        {
            const cells: CalendarCell[] = this.columns.map(column =>
            {
                const items = this.getItemsForHourAndColumn(hour, column.dateLocationKey);
                return {
                    dateLocationKey: column.dateLocationKey,
                    items,
                    isEmpty: items.length === 0
                };
            });

            return {
                hour,
                displayHour: this.formatHour(hour),
                cells
            };
        });
    }

    /**
     * Get items that occur during a specific hour for a specific date/location
     */
    private getItemsForHourAndColumn(hour: number, dateLocationKey: string): CalendarIcsItem[]
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
     * Track by function for columns
     */
    trackByColumn(index: number, column: CalendarColumn): string
    {
        return column.dateLocationKey;
    }

    /**
     * Track by function for rows
     */
    trackByRow(index: number, row: CalendarRow): number
    {
        return row.hour;
    }

    /**
     * Track by function for items
     */
    trackByItem(index: number, item: CalendarIcsItem): string
    {
        return item.id || `${index}_${item.registrableId}_${item.start}`;
    }
}