import { Injectable } from '@angular/core';
import { Api, EventSetupState } from 'app/api/api';
import { FetchService } from '../infrastructure/fetchService';
import { EventService } from '../events/event.service';
import { NotificationService } from '../infrastructure/notification.service';
import { Observable } from 'rxjs';
import { Clipboard } from '@angular/cdk/clipboard';

@Injectable({
  providedIn: 'root'
})
export class SetupEventService extends FetchService<EventSetupState>
{

  constructor(private api: Api,
    private eventService: EventService,
    notificationService: NotificationService,
    private clipboard: Clipboard)
  {
    super('EventSetupStateQuery', notificationService);
  }

  get state$(): Observable<EventSetupState>
  {
    return this.result$;
  }

  fetchState(): Observable<EventSetupState>
  {
    return this.fetchItems(this.api.eventSetupState_Query({ eventId: this.eventService.selectedId }), null, this.eventService.selectedId);
  }

  copyScriptToClipboard(): void
  {
    this.api.googleFormsScript_Query({ eventId: this.eventService.selectedId })
      .subscribe(script => this.clipboard.copy(script));
  }
}
