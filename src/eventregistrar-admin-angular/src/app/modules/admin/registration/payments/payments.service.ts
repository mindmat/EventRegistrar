import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, map, Observable } from 'rxjs';
import { EventService } from '../../events/event.service';

@Injectable({
  providedIn: 'root'
})
export class PaymentsService
{
  private payments: BehaviorSubject<PaymentOfRegistration[] | null> = new BehaviorSubject(null);

  constructor(private httpClient: HttpClient, private eventService: EventService) { }

  get payments$(): Observable<PaymentOfRegistration[]>
  {
    return this.payments.asObservable();
  }

  fetchPaymentsOfRegistration(registrationId: string): Observable<PaymentOfRegistration[]>
  {
    return this.httpClient.get<PaymentOfRegistration[]>(`api/events/${this.eventService.selected}/registrations/${registrationId}/assignedPayments`).pipe(
      map(reg =>
      {
        this.payments.next(reg);
        return reg;
      })
    );
  }
}


export class PaymentOfRegistration
{
  paymentAssignmentId: string;
  amount: number;
  bookingDate: Date;
  currency: string;
  paymentAssignmentId_Counter: string;
}