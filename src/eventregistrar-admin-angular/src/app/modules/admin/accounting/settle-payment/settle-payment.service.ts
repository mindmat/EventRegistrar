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
  private _paymentId: string;
  private searchString: string;

  constructor(private api: Api, private eventService: EventService, notificationService: NotificationService)
  {
    super('PaymentAssignmentsQuery', notificationService);
  }

  public get paymentId(): string
  {
    return this._paymentId;
  }

  get candidates$(): Observable<PaymentAssignments>
  {
    return this.result$;
  }

  fetchCandidates(paymentId: string, searchString: string | null = null): Observable<PaymentAssignments>
  {
    this._paymentId = paymentId;
    return this.fetchItems(this.api.paymentAssignments_Query({ eventId: this.eventService.selectedId, paymentId, searchString }), paymentId, this.eventService.selectedId);
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

  assignRepayment(incomingPaymentId: string, outgoingPaymentId: string, amountToAssign: number)
  {
    this.api.assignRepayment_Command({ eventId: this.eventService.selectedId, incomingPaymentId, outgoingPaymentId, amount: amountToAssign })
      .subscribe(x => console.log(x));
  }

  assignPayoutRequest(paymentId: string, payoutRequestId: string, amountToAssign: number)
  {
    this.api.assignOutgoingPayment_Command({ eventId: this.eventService.selectedId, outgoingPaymentId: paymentId, payoutRequestId, amount: amountToAssign })
      .subscribe(x => console.log(x));
  }


  ignorePayment(paymentId: string)
  {
    this.api.ignorePayment_Command({ eventId: this.eventService.selectedId, paymentId })
      .subscribe();
  }
}
