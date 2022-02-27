import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ListService } from '../../infrastructure/listService';

@Injectable({
  providedIn: 'root'
})
export class SettlePaymentService
{
  constructor(private service: ListService<AssignmentCandidate>) { }

  get candidates$(): Observable<AssignmentCandidate[]>
  {
    return this.service.list$;
  }

  fetchCandidates(id: string)
  {
    return this.service.fetchItems(`accounting/bankAccountBookingId/${id}/assignmentCandidates`);
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
