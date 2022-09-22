import { Injectable } from '@angular/core';
import { Api, PaymentAssignments, PaymentType } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../../events/event.service';
import { FetchService } from '../../infrastructure/fetchService';
import { NotificationService } from '../../infrastructure/notification.service';

@Injectable({
  providedIn: 'root'
})
export class SettlePaymentService extends FetchService<PaymentAssignments>
{
  private paymentId: string;

  constructor(private api: Api,
    private eventService: EventService,
    notificationService: NotificationService)
  {
    super('PaymentAssignmentsQuery', notificationService);
  }

  get candidates$(): Observable<PaymentAssignments>
  {
    return this.result$;
  }

  fetchCandidates(paymentId: string)
  {
    this.paymentId = paymentId;
    return this.fetchItems(this.api.paymentAssignments_Query({ eventId: this.eventService.selectedId, paymentId }), this.paymentId);
  }

  unassign(paymentAssignmentId: string)
  {
    return this.api.unassignPayment_Command({ eventId: this.eventService.selectedId, paymentAssignmentId: paymentAssignmentId })
      .subscribe(x => console.log(x));
  }

  assign(paymentType: PaymentType, paymentId: string, registrationId: string, amount: number,
    acceptDifference: boolean, acceptDifferenceReason: string)
  {
    if (paymentType === PaymentType.Incoming)
    {
      this.api.assignIncomingPayment_Command({ eventId: this.eventService.selectedId, paymentIncomingId: paymentId, registrationId: registrationId, amount: amount, acceptDifference: acceptDifference, acceptDifferenceReason: acceptDifferenceReason })
        .subscribe(x => console.log(x));
    }
    else
    {
      this.api.assignOutgoingPayment_Command({ eventId: this.eventService.selectedId, outgoingPaymentId: paymentId, registrationId: registrationId, amount: amount, acceptDifference: acceptDifference, acceptDifferenceReason: acceptDifferenceReason })
        .subscribe(x => console.log(x));
    }
  }
}
