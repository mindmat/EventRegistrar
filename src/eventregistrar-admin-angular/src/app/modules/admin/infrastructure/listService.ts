import { BehaviorSubject, map, Observable, of, switchMap, throwError } from 'rxjs';

export class ListService<TListItem>
{
    private list: BehaviorSubject<TListItem[] | null> = new BehaviorSubject(null);

    protected get list$(): Observable<TListItem[]>
    {
        return this.list.asObservable();
    }

    protected fetchItems(apiCall: Observable<TListItem[]>): Observable<TListItem[]>
    {
        return apiCall.pipe(
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

