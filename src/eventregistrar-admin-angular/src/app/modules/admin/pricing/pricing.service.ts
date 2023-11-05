import { Injectable } from '@angular/core';
import { Api, PricePackageDto } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../events/event.service';
import { FetchService } from '../infrastructure/fetchService';
import { NotificationService } from '../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class PricingService extends FetchService<PricePackageDto[]> {

  constructor(private api: Api,
    private eventService: EventService,
    notificationService: NotificationService)
  {
    super('PricingQuery', notificationService);
  }

  get pricePackages$(): Observable<PricePackageDto[]>
  {
    return this.result$;
  }

  fetchPricing(): Observable<PricePackageDto[]>
  {
    return this.fetchItems(this.api.pricing_Query({ eventId: this.eventService.selectedId }), null, this.eventService.selectedId);
  }

  save(packages: PricePackageDto[]): void
  {
    this.api.savePricing_Command({ eventId: this.eventService.selectedId, packages })
      .subscribe();
  }
}
