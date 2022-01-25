import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ListByIdService } from '../../infrastructure/listByIdService';

@Injectable({
  providedIn: 'root'
})
export class BankStatementsService
{
  constructor(private service: ListByIdService<BankAccountBooking>) { }

  get payments$(): Observable<BankAccountBooking[]>
  {
    return this.service.list$;
  }

  fetchAllBankStatements(id: string): Observable<BankAccountBooking[]>
  {
    return this.service.fetchItemsOf(id, 'bank-statements');
  }
}


export class BankAccountBooking
{
  id: string;
  amount: number;
  amountAssigned: number;
  bookingDate: Date;
  currency: string;
  info: string;
  reference: string;
  amountRepaid: number;
  settled: boolean;
  locked: boolean;
  ignore: boolean;
}