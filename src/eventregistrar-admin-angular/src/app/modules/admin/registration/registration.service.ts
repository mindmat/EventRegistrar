import { Injectable } from '@angular/core';
import { Api, RegistrationDisplayItem } from 'app/api/api';
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
}