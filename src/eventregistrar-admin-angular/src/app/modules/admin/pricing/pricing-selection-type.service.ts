import { Injectable } from '@angular/core';
import { Api, PricePackagePartSelectionTypeOption } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../events/event.service';
import { FetchService } from '../infrastructure/fetchService';
import { NotificationService } from '../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class PricePackagePartSelectionTypeService extends FetchService<PricePackagePartSelectionTypeOption[]> {

  constructor(private api: Api,
    private eventService: EventService,
    notificationService: NotificationService)
  {
    super('PricePackagePartSelectionTypeQuery', notificationService);
  }

  get selectionTypes$(): Observable<PricePackagePartSelectionTypeOption[]>
  {
    return this.result$;
  }

  fetchSelectionTypes()
  {
    return this.fetchItems(this.api.pricePackagePartSelectionType_Query({}), null, this.eventService.selectedId);
  }
}