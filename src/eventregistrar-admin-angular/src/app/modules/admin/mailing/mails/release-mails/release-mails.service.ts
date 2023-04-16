import { Injectable } from '@angular/core';
import { Api, PendingMailListItem } from 'app/api/api';
import { EventService } from 'app/modules/admin/events/event.service';
import { FetchService } from 'app/modules/admin/infrastructure/fetchService';
import { NotificationService } from 'app/modules/admin/infrastructure/notification.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ReleaseMailsService extends FetchService<PendingMailListItem[]>
{
  constructor(private api: Api,
    private eventService: EventService,
    notificationService: NotificationService)
  {
    super('PendingMailsQuery', notificationService);
  }

  get pendingMails$(): Observable<PendingMailListItem[]>
  {
    return this.result$;
  }

  fetchPendingMails()
  {
    return this.fetchItems(this.api.pendingMails_Query({ eventId: this.eventService.selectedId }), null, this.eventService.selectedId);
  }

  releaseMails(mailIds: string[])
  {
    this.api.releaseMails_Command({ eventId: this.eventService.selectedId, mailIds })
      .subscribe();
  }

  deleteMails(mailIds: string[])
  {
    this.api.deleteMails_Command({ eventId: this.eventService.selectedId, mailIds })
      .subscribe();
  }
}
