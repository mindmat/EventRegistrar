import { Injectable } from '@angular/core';
import { Api, MailType, MailTypeItem, RegistrationDisplayItem, Unit } from 'app/api/api';
import { tap, BehaviorSubject, filter, map, Observable } from 'rxjs';
import { EventService } from '../events/event.service';
import { NotificationService } from '../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class RegistrationService
{
  private registration: BehaviorSubject<RegistrationDisplayItem | null> = new BehaviorSubject(null);
  private registrationId: string;

  constructor(
    private api: Api,
    private eventService: EventService,
    notificationService: NotificationService)
  {
    notificationService.subscribe('RegistrationQuery').pipe(
      filter(e => e.rowId === this.registrationId),
    )
      .subscribe(e => this.refresh());
  }

  refresh(): void
  {
    this.api.registration_Query({ eventId: this.eventService.selectedId, registrationId: this.registrationId })
      .subscribe(reg => this.registration.next(reg));
  }

  get registration$(): Observable<RegistrationDisplayItem>
  {
    return this.registration.asObservable();
  }

  fetchRegistration(registrationId: string): Observable<RegistrationDisplayItem>
  {
    this.registrationId = registrationId;
    return this.api.registration_Query({ eventId: this.eventService.selectedId, registrationId: registrationId }).pipe(
      map(reg =>
      {
        this.registration.next(reg);
        return reg;
      })
    );
  }

  getPossibleMailTypes(registrationId: string): Observable<MailTypeItem[]>
  {
    return this.api.possibleMailTypes_Query({ eventId: this.eventService.selectedId, registrationId: registrationId });
  }

  createAutoMail(registrationId: string, mailType: MailType): Observable<any>
  {
    return this.api.composeAndSendAutoMail_Command(
      {
        eventId: this.eventService.selectedId,
        registrationId,
        allowDuplicate: true,
        withhold: true,
        mailType
      });
  }

  createBulkMail(registrationId: string, bulkMailKey: string): Observable<any>
  {
    return this.api.composeAndSendBulkMail_Command(
      {
        eventId: this.eventService.selectedId,
        registrationId,
        allowDuplicate: true,
        withhold: true,
        bulkMailKey
      });
  }

  updateNotes(registrationId: string, notes: string | null): Observable<any>
  {
    return this.api.updateInternalNotes_Command({ eventId: this.eventService.selectedId, registrationId, notes });
  }

  unbindPartnerRegistrations(registrationId: string)
  {
    return this.api.unbindPartnerRegistration_Command({ eventId: this.eventService.selectedId, registrationId })
      .subscribe();
  }
}