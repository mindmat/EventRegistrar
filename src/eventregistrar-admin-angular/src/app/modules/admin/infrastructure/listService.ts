import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, map, Observable, of, switchMap, throwError } from 'rxjs';
import { EventService } from '../events/event.service';

export class ListService<TListItem>
{
    private list: BehaviorSubject<TListItem[] | null> = new BehaviorSubject(null);

    constructor(protected httpClient: HttpClient, private eventService: EventService) { }

    protected get list$(): Observable<TListItem[]>
    {
        return this.list.asObservable();
    }

    protected fetchItems(urlInEvent?: string, url?: string, params?: any): Observable<TListItem[]>
    {
        url = url ?? this.getEventUrl(urlInEvent);
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

    protected getEventUrl(urlInEvent: string)
    {
        return `api/events/${this.eventService.selected}/${urlInEvent}`;
    }
}

