import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ListByIdService } from '../../infrastructure/listByIdService';

@Injectable({
  providedIn: 'root'
})
export class BankStatementsService
{
  constructor(private service: ListByIdService<BookingsOfDay>) { }

  get payments$(): Observable<BookingsOfDay[]>
  {
    return this.service.list$;
  }

  fetchAllBankStatements(id: string): Observable<BookingsOfDay[]>
  {
    return this.service.fetchItemsOf(id, 'bank-statements');
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
  debitorName: string;
  creditorName: string;
  amount: number;
  charges: number;
  amountAssigned: number;
  bookingDate: Date;
  currency: string;
  message: string;
  reference: string;
  amountRepaid: number;
  settled: boolean;
  locked: boolean;
  ignore: boolean;
}