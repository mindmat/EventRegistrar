import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, map, Observable, of, switchMap, throwError } from 'rxjs';
import { EventService } from '../events/event.service';

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

