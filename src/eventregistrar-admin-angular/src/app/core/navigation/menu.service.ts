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
        private eventService: EventService,
        notificationService: NotificationService)
    {
        super('MenuNodesQuery', notificationService);
    }

    get nodeContents$(): Observable<MenuNodeContent[]>
    {
        return this.result$;
    }

    fetchMenuItems()
    {
        return this.fetchItems(this.api.menuNodes_Query({ eventId: this.eventService.selectedId }), null, this.eventService.selectedId);
    }
}
