import { Injectable } from '@angular/core';
import { Api, PricePackageOverview } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../events/event.service';
import { FetchService } from '../infrastructure/fetchService';
import { NotificationService } from '../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class PricePackagesOverviewService extends FetchService<PricePackageOverview | null>
{
  constructor(private api: Api, private eventService: EventService, notificationService: NotificationService)
  {
    super('PricePackageOverviewQuery', notificationService);
  }

  get pricePackageOverview$(): Observable<PricePackageOverview | null>
  {
    return this.result$;
  }

  fetchData(): Observable<any>
  {
    return this.fetchItems(this.api.pricePackageOverview_Query({ eventId: this.eventService.selectedId }), null, this.eventService.selectedId);
  }
}
