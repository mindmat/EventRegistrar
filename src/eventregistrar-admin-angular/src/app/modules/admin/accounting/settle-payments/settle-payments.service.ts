import { ChangeDetectorRef, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ListService } from '../../infrastructure/listService';
import { BookingsOfDay, CreditDebit } from '../bankStatements/bankStatements.service';

@Injectable({
  providedIn: 'root'
})
export class SettlePaymentsService
{
  constructor(private service: ListService<BookingsOfDay>) { }

  get payments$(): Observable<BookingsOfDay[]>
  {
    return this.service.list$;
  }

  fetchBankStatements(hideIncoming: boolean = false, hideOutgoing: boolean = false, hideSettled: boolean = true, hideIgnored: boolean = true): Observable<BookingsOfDay[]>
  {
    return this.service.fetchItems('accounting/bank-statements', null, { hideIncoming, hideOutgoing, hideSettled, hideIgnored });
  }
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


export class PossibleAssignment
{
  firstName: string;
  lastName: string;
  paymentId: string;
  registrationId: string;
  amount: number;
  amountPaid: number;
  amountToAssign: number;
  acceptDifference: boolean;
  acceptDifferenceReason: string;
  locked: boolean;
  isWaitingList: boolean;
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