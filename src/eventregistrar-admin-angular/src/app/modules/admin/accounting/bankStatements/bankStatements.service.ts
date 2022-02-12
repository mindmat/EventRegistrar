import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ListService } from '../../infrastructure/listService';

@Injectable({
  providedIn: 'root'
})
export class BankStatementsService
{
  constructor(private service: ListService<BookingsOfDay>) { }

  get payments$(): Observable<BookingsOfDay[]>
  {
    return this.service.list$;
  }

  fetchAllBankStatements(): Observable<BookingsOfDay[]>
  {
    return this.service.fetchItems('accounting/bank-statements', null, { hideIncoming: false, hideOutgoing: false, hideSettled: false, hideIgnored: true });
  }
}

export enum CreditDebit
{
  CRDT = 1,
  DBIT = 2
}

export class BookingsOfDay
{
  bookingDate: Date;
  balanceAfter: number;
  bookings: BankAccountBooking[];
}

export class BankAccountBooking
{
  id: string;
  typ: CreditDebit;
  bookingDate: Date;
  amount: number;
  charges: number;
  currency: string;
  debitorName: string;
  creditorName: string;
  creditorIban: string;
  message: string;
  reference: string;
  paymentSlipId: string;

  amountAssigned: number;
  amountRepaid: number;
  settled: boolean;
  ignore: boolean;
}