import { BehaviorSubject, map, Observable, of, switchMap, throwError } from 'rxjs';

export class FetchService<TItem>
{
    private result: BehaviorSubject<TItem | null> = new BehaviorSubject(null);

    protected get result$(): Observable<TItem>
    {
        return this.result.asObservable();
    }

    protected fetchItems(fetch: Observable<TItem>): Observable<TItem>
    {
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

