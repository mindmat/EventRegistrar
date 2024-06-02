import { Injectable } from '@angular/core';
import { FetchService } from '../../infrastructure/fetchService';
import { Api, BankAccountConfiguration } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../../events/event.service';
import { NotificationService } from '../../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class AccountConfigService extends FetchService<BankAccountConfiguration>
{
  constructor(private api: Api,
    private eventService: EventService,
    notificationService: NotificationService)
  {
    super('BankAccountConfigurationQuery', notificationService);
  }

  get bankAccount$(): Observable<BankAccountConfiguration>
  {
    return this.result$;
  }

  fetchConfig(): Observable<any>
  {
    return this.fetchItems(this.api.bankAccountConfiguration_Query({ eventId: this.eventService.selectedId }), null, this.eventService.selectedId);
  }

  save(config: BankAccountConfiguration): void
  {
    this.api.saveBankAccountConfiguration_Command({ eventId: this.eventService.selectedId, config })
      .subscribe();
  };
}
