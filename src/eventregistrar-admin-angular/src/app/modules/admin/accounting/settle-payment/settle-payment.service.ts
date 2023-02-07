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
  public get paymentId(): string
  {
    return this._paymentId;
  }
  private set paymentId(value: string)
  {
    this._paymentId = value;
  }

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
    return this.fetchItems(this.api.paymentAssignments_Query({ eventId: this.eventService.selectedId, paymentId }), this.paymentId, this.eventService.selectedId);
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
