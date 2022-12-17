import { Injectable } from '@angular/core';
import { Api, MailView } from 'app/api/api';
import { EventService } from 'app/modules/admin/events/event.service';
import { FetchService } from 'app/modules/admin/infrastructure/fetchService';
import { NotificationService } from 'app/modules/admin/infrastructure/notification.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class MailService extends FetchService<MailView>
{
  constructor(private api: Api,
    private eventService: EventService,
    notificationService: NotificationService)
  {
    super('MailViewQuery', notificationService);
  }

  get mail$(): Observable<MailView>
  {
    return this.result$;
  }

  fetchMail(mailId: string)
  {
    return this.fetchItems(this.api.mailView_Query({ eventId: this.eventService.selectedId, mailId }), mailId, this.eventService.selectedId);
  }

  releaseMail(mailId: string)
  {
    this.api.releaseMail_Command({ eventId: this.eventService.selectedId, mailId })
      .subscribe();
  }

  deleteMail(mailId: string)
  {
    this.api.deleteMail_Command({ eventId: this.eventService.selectedId, mailId })
      .subscribe();
  }
}
