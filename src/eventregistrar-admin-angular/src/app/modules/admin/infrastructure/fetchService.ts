import { BehaviorSubject, filter, map, Observable, of, switchMap, throwError } from 'rxjs';
import { NotificationService } from './notification.service';

export class FetchService<TItem>
{
    private result: BehaviorSubject<TItem | null> = new BehaviorSubject(null);
    private fetch: Observable<TItem>;
    private rowId?: string;
    private eventId?: string;

    constructor(queryName: string | null = null, notificationService: NotificationService | null = null)
    {
        if (queryName != null && notificationService != null)
        {
            notificationService?.subscribe(queryName)
                .pipe(
                    filter(e => e.rowId === this.rowId && e.eventId === this.eventId)
                )
                .subscribe(_ => this.refresh());
        }
    }

    refresh(): void
    {
        this.fetch.subscribe(result => this.result.next(result));
    }

    protected get result$(): Observable<TItem>
    {
        return this.result.asObservable();
    }

    protected fetchItems(fetch: Observable<TItem>, rowId: string | null = null, eventId: string | null = null): Observable<TItem>
    {
        this.rowId = rowId;
        this.eventId = eventId;
        this.fetch = fetch;
        return fetch.pipe(
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
}

