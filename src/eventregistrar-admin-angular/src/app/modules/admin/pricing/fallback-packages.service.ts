import { Injectable } from '@angular/core';
import { Api, FallbackPricePackage } from 'app/api/api';
import { EventService } from '../events/event.service';
import { FetchService } from '../infrastructure/fetchService';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class FallbackPackagesService extends FetchService<FallbackPricePackage[]> {

  constructor(private api: Api,
    private eventService: EventService)
  {
    super();
  }

  get possiblePackages$(): Observable<FallbackPricePackage[]>
  {
    return this.result$;
  }

  getPossiblePackages(registrationId: string)
  {
    return this.fetchItems(this.api.possibleManualFallbackPricePackages_Query({ registrationId, eventId: this.eventService.selectedId }), null, this.eventService.selectedId);
  }
}