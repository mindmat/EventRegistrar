import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, map, Observable, of, switchMap, throwError } from 'rxjs';
import { EventService } from '../events/event.service';

@Injectable({
    providedIn: 'root'
})
export class ListByIdService<TListItem>
{
    private list: BehaviorSubject<TListItem[] | null> = new BehaviorSubject(null);

    constructor(private httpClient: HttpClient, private eventService: EventService) { }

    get list$(): Observable<TListItem[]>
    {
        return this.list.asObservable();
    }

    fetchItemsOf(id: string, urlInEvent?: string, url?: string): Observable<TListItem[]>
    {
        url = url ?? `api/events/${this.eventService.selected}/${urlInEvent}`;
        return this.httpClient.get<TListItem[]>(url).pipe(
            map(newItems =>
            {
                // Update the course
                this.list.next(newItems);

                // Return the course
                return newItems;
            }),
            switchMap(newItems =>
            {
                if (!newItems)
                {
                    return throwError(() => `Could not find items with id of ${id}!`);
                }

                return of(newItems);
            })
        );
    }
}

