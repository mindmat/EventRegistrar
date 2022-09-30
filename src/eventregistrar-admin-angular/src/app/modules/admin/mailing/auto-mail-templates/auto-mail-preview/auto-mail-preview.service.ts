import { Injectable } from '@angular/core';
import { Api, AutoMailPreview } from 'app/api/api';
import { EventService } from 'app/modules/admin/events/event.service';
import { FetchService } from 'app/modules/admin/infrastructure/fetchService';
import { NotificationService } from 'app/modules/admin/infrastructure/notification.service';
import { BehaviorSubject, combineLatest, filter, Observable, of, switchMap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AutoMailPreviewService extends FetchService<AutoMailPreview> {

  private autoMailTemplateId: BehaviorSubject<string | null> = new BehaviorSubject(null);
  private registrationId: BehaviorSubject<string | null> = new BehaviorSubject(null);

  constructor(private api: Api,
    private eventService: EventService,
    notificationService: NotificationService)
  {
    super('AutoMailPreviewQuery', notificationService);

    combineLatest([this.registrationId, this.autoMailTemplateId]).pipe(
      filter(([_, tid]) => tid != null),
      switchMap(([rid, tid]) => this.fetchItems(this.api.autoMailPreview_Query({ eventId: this.eventService.selectedId, autoMailTemplateId: tid, registrationId: rid }), tid, this.eventService.selectedId))
    ).subscribe(x => console.log(x));
  }

  get preview$(): Observable<AutoMailPreview>
  {
    return this.result$;
  }

  setRegistrationId(registrationId: string)
  {
    this.registrationId.next(registrationId);
  }

  setTemplateId(autoMailTemplateId: string)
  {
    this.autoMailTemplateId.next(autoMailTemplateId);
    return of(null);
  }
}