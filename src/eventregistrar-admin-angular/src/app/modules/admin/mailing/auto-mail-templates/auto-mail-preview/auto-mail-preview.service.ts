import { Injectable } from '@angular/core';
import { Api, MailTemplatePreview } from 'app/api/api';
import { EventService } from 'app/modules/admin/events/event.service';
import { FetchService } from 'app/modules/admin/infrastructure/fetchService';
import { NotificationService } from 'app/modules/admin/infrastructure/notification.service';
import { BehaviorSubject, combineLatest, filter, Observable, of, switchMap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AutoMailPreviewService extends FetchService<MailTemplatePreview> {

  private mailTemplateId: BehaviorSubject<string | null> = new BehaviorSubject(null);
  private registrationId: BehaviorSubject<string | null> = new BehaviorSubject(null);

  constructor(private api: Api,
    private eventService: EventService,
    notificationService: NotificationService)
  {
    super('MailTemplatePreviewQuery', notificationService);

    combineLatest([this.registrationId, this.mailTemplateId]).pipe(
      filter(([_, tid]) => tid != null),
      switchMap(([rid, tid]) => this.fetchItems(this.api.mailTemplatePreview_Query({ eventId: this.eventService.selectedId, mailTemplateId: tid, registrationId: rid }), tid, this.eventService.selectedId))
    ).subscribe(x => console.log(x));
  }

  get preview$(): Observable<MailTemplatePreview>
  {
    return this.result$;
  }

  setRegistrationId(registrationId: string)
  {
    this.registrationId.next(registrationId);
  }

  setTemplateId(autoMailTemplateId: string)
  {
    this.mailTemplateId.next(autoMailTemplateId);
    return of(null);
  }
}