import { Injectable } from '@angular/core';
import { Api, BookingAssignments, PaymentType } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../../events/event.service';
import { FetchService } from '../../infrastructure/fetchService';

@Injectable({
  providedIn: 'root'
})
export class SettlePaymentService extends FetchService<BookingAssignments>
{
  constructor(private api: Api, private eventService: EventService) { super(); }

  get candidates$(): Observable<BookingAssignments>
  {
    return this.result$;
  }

  fetchCandidates(id: string)
  {
    return this.fetchItems(this.api.possibleAssignments_Query({ eventId: this.eventService.selectedId, paymentId: id }));
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
