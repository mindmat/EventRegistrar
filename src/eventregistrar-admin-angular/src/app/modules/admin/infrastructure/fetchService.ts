import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, map, Observable, of, switchMap, throwError } from 'rxjs';
import { EventService } from '../events/event.service';

export class FetchService<TItem>
{
    private result: BehaviorSubject<TItem | null> = new BehaviorSubject(null);

    constructor(protected httpClient: HttpClient, private eventService: EventService) { }

    protected get result$(): Observable<TItem>
    {
        return this.result.asObservable();
    }

    protected fetchItems(urlInEvent?: string, url?: string, params?: any): Observable<TItem>
    {
        url = url ?? this.getEventUrl(urlInEvent);
        const options = params ? { params } : {};
        return this.httpClient.get<TItem>(url, options).pipe(
            map(newItems =>
            {

                // Update the item
                this.result.next(newItems);

                // Return the item
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

