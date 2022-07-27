import { Injectable } from '@angular/core';
import { Api, BookingsOfDay } from 'app/api/api';
import { Observable } from 'rxjs';
import { EventService } from '../../events/event.service';
import { ListService } from '../../infrastructure/listService';

@Injectable({
  providedIn: 'root'
})
export class SettlePaymentsService extends ListService<BookingsOfDay>
{
  constructor(private api: Api, private eventService: EventService) { super(); }

  get payments$(): Observable<BookingsOfDay[]>
  {
    return this.list$;
  }

  // get candidates$(): Observable<AssignmentCandidate[]>
  // {
  //   return this.candidatesService.list$;
  // }

  fetchBankStatements(searchString: string = null, hideIncoming: boolean = false, hideOutgoing: boolean = false, hideSettled: boolean = true, hideIgnored: boolean = true): Observable<BookingsOfDay[]>
  {
    return this.fetchItems(this.api.paymentsByDay_Query({ eventId: this.eventService.selectedId, searchString, hideIncoming, hideOutgoing, hideSettled, hideIgnored }));
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

