import { HttpClient } from '@angular/common/http';
import { ChangeDetectorRef, Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { EventService } from '../../events/event.service';
import { ListService } from '../../infrastructure/listService';
import { BookingsOfDay, CreditDebit } from '../bankStatements/bankStatements.service';

@Injectable({
  providedIn: 'root'
})
export class SettlePaymentsService extends ListService<BookingsOfDay>
{
  constructor(httpClient: HttpClient, eventService: EventService) { super(httpClient, eventService); }

  get payments$(): Observable<BookingsOfDay[]>
  {
    return this.list$;
  }

  // get candidates$(): Observable<AssignmentCandidate[]>
  // {
  //   return this.candidatesService.list$;
  // }

  fetchBankStatements(hideIncoming: boolean = false, hideOutgoing: boolean = false, hideSettled: boolean = true, hideIgnored: boolean = true): Observable<BookingsOfDay[]>
  {
    return this.fetchItems('accounting/bank-statements', null, { hideIncoming, hideOutgoing, hideSettled, hideIgnored });
  }

  // fetchCandidates(id?: string): Observable<AssignmentCandidate[]>
  // {
  //   if (!id)
  //   {
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