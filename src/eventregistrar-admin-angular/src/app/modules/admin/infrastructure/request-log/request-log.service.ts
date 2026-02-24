import { HttpClient } from '@angular/common/http';
import { Inject, Injectable, Optional } from '@angular/core';
import { Api, API_BASE_URL, RequestLogDisplayItem, RequestLogQuery } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../../events/event.service';
import { FetchService } from '../fetchService';
import { NotificationService } from '../notification.service';

export interface RequestTypeOption
{
    requestType: string;
    requestTypeText: string;
}

@Injectable({
    providedIn: 'root'
})
export class RequestLogService extends FetchService<RequestLogDisplayItem[]>
{
    constructor(private readonly api: Api,
        private readonly eventService: EventService,
        notificationService: NotificationService,
        @Inject(HttpClient) private readonly http: HttpClient,
        @Optional() @Inject(API_BASE_URL) private readonly baseUrl?: string)
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

    fetchRequestTypeOptions(): Observable<RequestTypeOption[]>
    {
        return this.http.post<RequestTypeOption[]>(`${this.baseUrl}/api/RequestLogRequestTypesQuery`, { eventId: this.eventService.selectedId });
    }
}
