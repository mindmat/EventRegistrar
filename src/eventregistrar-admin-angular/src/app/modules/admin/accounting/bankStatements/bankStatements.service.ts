import { Injectable } from '@angular/core';
import { Api, BookingsOfDay } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../../events/event.service';
import { ListService } from '../../infrastructure/listService';

@Injectable({
  providedIn: 'root'
})
export class BankStatementsService extends ListService<BookingsOfDay>
{
  constructor(private api: Api, private eventService: EventService) { super(); }

  get payments$(): Observable<BookingsOfDay[]>
  {
    return this.list$;
  }

  fetchBankStatements(hideIncoming: boolean = false, hideOutgoing: boolean = false, hideSettled: boolean = false, hideIgnored: boolean = false): Observable<BookingsOfDay[]>
  {
    return this.fetchItems(this.api.bankAccountBookings_Query({ eventId: this.eventService.selectedId, hideIncoming, hideOutgoing, hideSettled, hideIgnored }));
  }
}
