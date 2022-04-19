import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { EventService } from '../../events/event.service';
import { FetchService } from '../../infrastructure/fetchService';

@Injectable({
  providedIn: 'root'
})
export class SettlePaymentService extends FetchService<BookingAssignments>
{
  constructor(httpClient: HttpClient, eventService: EventService) { super(httpClient, eventService); }

  get candidates$(): Observable<BookingAssignments>
  {
    return this.result$;
  }

  fetchCandidates(id: string)
  {
    return this.fetchItems(`accounting/bankAccountBookingId/${id}/assignmentCandidates`);
  }

  unassign(paymentAssignmentId: string)
  {
    this.httpClient.delete(this.getEventUrl(`paymentAssignments/${paymentAssignmentId}`))
      .subscribe(x => console.log(x));
  }

  assign(bankAccountBookingId: string, registrationId: string, amount: number,
    acceptDifference: boolean, acceptDifferenceReason: string)
  {
    this.httpClient.post(this.getEventUrl(`payments/${bankAccountBookingId}/assign/${registrationId}`),
      { amount, acceptDifference, acceptDifferenceReason })
      .subscribe(x => console.log(x));
  }
}

export class BookingAssignments
{
  registrationCandidates: AssignmentCandidateRegistration[];
  existingAssignments: ExistingAssignment[];
}

export class AssignmentCandidateRegistration
{
  registrationId: string;

  firstName: string;
  lastName: string;
  email: string;
  price: number;
  isWaitingList: boolean;

  amountMatch: boolean;
  amountPaid: number;
  matchScore: number;

  bankAccountBookingId: string;

  amountToAssign: number;
  difference: number;
  acceptDifference: boolean;
  acceptDifferenceReason: string;
  locked: boolean;
}

export class ExistingAssignment
{
  registrationId: string;
  paymentAssignmentId_Existing: string;
  assignedAmount: number;

  firstName: string;
  lastName: string;
  email: string;
  price: number;
  isWaitingList: boolean;

  bankAccountBookingId: string;

  locked: boolean;
}

export class PossibleRepaymentAssignment
{
  amount: number;
  amountUnsettled: number;
  amountToAssign: number;
  bookingDate: Date;
  currency: string;
  debitorName: string;
  info: string;
  message: string;
  matchScore: number;
  paymentId_Counter: string;
  paymentId_OpenPosition: string;
  settled: boolean;
  locked: boolean;
}
