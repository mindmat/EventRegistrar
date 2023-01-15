import { Injectable } from '@angular/core';
import { Api, MailDisplayItem, MailDisplayType, MailView } from 'app/api/api';
import { EventService } from 'app/modules/admin/events/event.service';
import { FetchService } from 'app/modules/admin/infrastructure/fetchService';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class MailViewerService extends FetchService<MailView>
{
  constructor(private api: Api,
    private eventService: EventService)
  {
    super();
  }

  fetchMail(mailId: string): Observable<any>
  {
    return this.fetchItems(this.api.mailView_Query({ eventId: this.eventService.selectedId, mailId }));
  }

  get mail$(): Observable<MailView>
  {
    return this.result$;
  }
}
