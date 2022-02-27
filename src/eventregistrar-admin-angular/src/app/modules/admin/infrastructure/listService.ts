import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, map, Observable, of, switchMap, throwError } from 'rxjs';
import { EventService } from '../events/event.service';

@Injectable({
    providedIn: 'root'
})
export class ListService<TListItem>
{
    private list: BehaviorSubject<TListItem[] | null> = new BehaviorSubject(null);

    constructor(private httpClient: HttpClient, private eventService: EventService) { }

    get list$(): Observable<TListItem[]>
    {
        return this.list.asObservable();
    }

    fetchItems(urlInEvent?: string, url?: string, params?: any): Observable<TListItem[]>
    {
        url = url ?? `api/events/${this.eventService.selected}/${urlInEvent}`;
        const options = params ? { params } : {};
        return this.httpClient.get<TListItem[]>(url, options).pipe(
            map(newItems =>
            {

                // Update the list
                this.list.next(newItems);

                // Return the list
                return newItems;
            }),
            switchMap(newItems =>
            {
                if (!newItems)
                {
                    return throwError(() => 'Could not find any items!');
                }

                return of(newItems);
            })
        );
    }
}

