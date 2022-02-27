import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { EventService } from '../../events/event.service';
import { ListService } from '../../infrastructure/listService';

@Injectable({
  providedIn: 'root'
})
export class BankStatementsService extends ListService<BookingsOfDay>
{
  constructor(httpClient: HttpClient, eventService: EventService) { super(httpClient, eventService); }

  get payments$(): Observable<BookingsOfDay[]>
  {
    return this.list$;
  }

  fetchBankStatements(hideIncoming: boolean = false, hideOutgoing: boolean = false, hideSettled: boolean = false, hideIgnored: boolean = false): Observable<BookingsOfDay[]>
  {
    return this.fetchItems('accounting/bank-statements', null, { hideIncoming, hideOutgoing, hideSettled, hideIgnored });
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