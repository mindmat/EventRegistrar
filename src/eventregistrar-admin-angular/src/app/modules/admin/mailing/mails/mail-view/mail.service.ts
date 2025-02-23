import { HttpClient } from '@angular/common/http';
import { Inject, Injectable, Optional } from '@angular/core';
import { Api, API_BASE_URL, MailAttachmentMetadata, MailView } from 'app/api/api';
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
    notificationService: NotificationService,
    @Inject(HttpClient) private http: HttpClient,
    @Optional() @Inject(API_BASE_URL) private baseUrl?: string)
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
    this.api.releaseMails_Command({ eventId: this.eventService.selectedId, mailIds: [mailId] })
      .subscribe();
  }

  deleteMail(mailId: string)
  {
    this.api.deleteMails_Command({ eventId: this.eventService.selectedId, mailIds: [mailId] })
      .subscribe();
  }

  addIcs(mailId: string)
  {
    this.api.addIcsToMail_Command({ eventId: this.eventService.selectedId, mailId: mailId })
      .subscribe();
  }

  downloadAttachment(attachment: MailAttachmentMetadata)
  {
    const url = this.baseUrl + "/api/DownloadMailAttachmentQuery";
    this.http.post(url, { eventId: this.eventService.selectedId, mailAttachmentId: attachment.id }, { responseType: "blob" }).subscribe((file: Blob) =>
    {
      const blob = new Blob([file], { type: attachment.contentType });
      const anchor = window.document.createElement('a');
      anchor.href = window.URL.createObjectURL(blob);
      anchor.download = attachment.filename ?? 'attachment';
      document.body.appendChild(anchor);
      anchor.click();
      document.body.removeChild(anchor);
      window.URL.revokeObjectURL(anchor.href);
    });
  }
}
