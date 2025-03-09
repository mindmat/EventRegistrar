import { Injectable } from '@angular/core';
import { Api, MenuNodeContent } from 'app/api/api';
import { EventService } from 'app/modules/admin/events/event.service';
import { FetchService } from 'app/modules/admin/infrastructure/fetchService';
import { NotificationService } from 'app/modules/admin/infrastructure/notification.service';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class MenuService extends FetchService<MenuNodeContent[]>
{
    constructor(private api: Api,
        eventService: EventService,
        notificationService: NotificationService)
    {
        super('MenuNodesQuery', notificationService);
        eventService.selectedId$.subscribe(id => this.fetchMenuItems(id));
    }

    get nodeContents$(): Observable<MenuNodeContent[]>
    {
        return this.result$;
    }

    private fetchMenuItems(eventId?: string)
    {
        if (!eventId)
        {
            return;
        }
        return this.fetchItems(this.api.menuNodes_Query({ eventId }), null, eventId)
            .subscribe();
    }
}
