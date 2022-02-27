import { ChangeDetectorRef, Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { ListService } from '../../infrastructure/listService';
import { BookingsOfDay, CreditDebit } from '../bankStatements/bankStatements.service';

@Injectable({
  providedIn: 'root'
})
export class SettlePaymentsService
{
  constructor(private service: ListService<BookingsOfDay>)
  // private candidatesService: ListService<AssignmentCandidate>)
  {
  }

  get payments$(): Observable<BookingsOfDay[]>
  {
    return this.service.list$;
  }

  // get candidates$(): Observable<AssignmentCandidate[]>
  // {
  //   return this.candidatesService.list$;
  // }

  fetchBankStatements(hideIncoming: boolean = false, hideOutgoing: boolean = false, hideSettled: boolean = true, hideIgnored: boolean = true): Observable<BookingsOfDay[]>
  {
    console.log('fetchBankStatements');
    return this.service.fetchItems('accounting/bank-statements', null, { hideIncoming, hideOutgoing, hideSettled, hideIgnored });
  }

  // fetchCandidates(id?: string): Observable<AssignmentCandidate[]>
  // {
  //   if (!id)
  //   {
  //     console.log('fetchCandidates');
  //     return of(null);
  //   }
  //   return this.candidatesService.fetchItems(`accounting/bankAccountBookingId/${id}/assignmentCandidates`);
  // }
}


export class BankBookingDisplayItem
{
  id: string;
  typ: CreditDebit;
  amount: number;
  amountAssigned: number;
  //amountUnassigned: number;
  bookingDate: Date;
  currency: string;
  info: string;
  reference: string;
  repaid: number;
  settled: boolean;
  locked: boolean;
  paymentSlipId: string;
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