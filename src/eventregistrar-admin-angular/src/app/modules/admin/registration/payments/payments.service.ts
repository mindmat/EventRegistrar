import { Injectable } from '@angular/core';
import { Api, AssignedPaymentDisplayItem } from 'app/api/api';
import { BehaviorSubject, map, Observable } from 'rxjs';
import { EventService } from '../../events/event.service';

@Injectable({
  providedIn: 'root'
})
export class PaymentsService
{
  private payments: BehaviorSubject<AssignedPaymentDisplayItem[] | null> = new BehaviorSubject(null);

  constructor(private api: Api, private eventService: EventService) { }

  get payments$(): Observable<AssignedPaymentDisplayItem[]>
  {
    return this.payments.asObservable();
  }

  fetchPaymentsOfRegistration(registrationId: string): Observable<AssignedPaymentDisplayItem[]>
  {
    return this.api.assignedPaymentsOfRegistration_Query({ eventId: this.eventService.selectedId, registrationId }).pipe(
      map(reg =>
      {
        this.payments.next(reg);
        return reg;
      })
    );
  }
}
