import { Inject, Injectable, Optional } from '@angular/core';
import { API_BASE_URL, Api, HostingOffersAndRequests } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../../events/event.service';
import { FetchService } from '../../infrastructure/fetchService';
import { NotificationService } from '../../infrastructure/notification.service';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class HostingOverviewService extends FetchService<HostingOffersAndRequests>
{
  constructor(private api: Api,
    private eventService: EventService,
    notificationService: NotificationService,
    @Inject(HttpClient) private http: HttpClient,
    @Optional() @Inject(API_BASE_URL) private baseUrl?: string) 
  {
    super('HostingQuery', notificationService);
  }

  get hosting$(): Observable<HostingOffersAndRequests>
  {
    return this.result$;
  }

  fetchHosting(): Observable<HostingOffersAndRequests>
  {
    return this.fetchItems(this.api.hosting_Query({ eventId: this.eventService.selectedId }), null, this.eventService.selectedId);
  }

  downloadHostingXlsx()
  {
    const url = this.baseUrl + "/api/HostingQuery";
    const formatXlsx = 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet';
    this.http.post(url, { eventId: this.eventService.selectedId }, { responseType: "blob", headers: { 'Accept': formatXlsx } }).subscribe((file: Blob) =>
    {
      const blob = new Blob([file], { type: formatXlsx });
      const anchor = window.document.createElement('a');
      anchor.href = window.URL.createObjectURL(blob);
      anchor.download = 'hosting.xlsx';
      document.body.appendChild(anchor);
      anchor.click();
      document.body.removeChild(anchor);
      window.URL.revokeObjectURL(anchor.href);
    });
  }
}
