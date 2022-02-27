import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { EventService } from '../../events/event.service';
import { ListService } from '../../infrastructure/listService';

@Injectable({
  providedIn: 'root'
})
export class SettlePaymentService extends ListService<AssignmentCandidate>
{
  constructor(httpClient: HttpClient, eventService: EventService) { super(httpClient, eventService); }

  get candidates$(): Observable<AssignmentCandidate[]>
  {
    return this.list$;
  }

  fetchCandidates(id: string)
  {
    return this.fetchItems(`accounting/bankAccountBookingId/${id}/assignmentCandidates`);
  }
}


export class AssignmentCandidate
{
  registrationId: string;
  firstName: string;
  lastName: string;
  email: string;
  bankAccountBookingId: string;
  price: number;
  isWaitingList: boolean;

  amountPaid: number;
  amountToAssign: number;
  acceptDifference: boolean;
  acceptDifferenceReason: string;
  locked: boolean;
  matchScore: number;
  amountMatch: boolean;
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
