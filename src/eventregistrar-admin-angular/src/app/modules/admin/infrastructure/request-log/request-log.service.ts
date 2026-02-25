import { Injectable } from '@angular/core';
import { Api, RequestLogDisplayItem, RequestLogQuery, RequestTypeDisplayItem } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../../events/event.service';
import { FetchService } from '../fetchService';
import { NotificationService } from '../notification.service';

@Injectable({
    providedIn: 'root'
})
export class RequestLogService extends FetchService<RequestLogDisplayItem[]>
{
    constructor(private readonly api: Api,
        private readonly eventService: EventService,
        notificationService: NotificationService)
    {
        super('RequestLogQuery', notificationService);
    }

    get requestLogs$(): Observable<RequestLogDisplayItem[]>
    {
        return this.result$;
    }

    fetchRequestLog(query: RequestLogQuery)
    {
        return this.fetchItems(this.api.requestLog_Query(query), null, this.eventService.selectedId);
    }

    fetchRequestTypeOptions(): Observable<RequestTypeDisplayItem[]>
    {
        return this.api.requestLogRequestTypes_Query({ eventId: this.eventService.selectedId ?? undefined });
    }
}
