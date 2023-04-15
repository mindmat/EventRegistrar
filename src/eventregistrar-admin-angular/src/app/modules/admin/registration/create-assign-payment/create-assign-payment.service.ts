import { Injectable } from '@angular/core';
import { Api, CreateAndAssignIncomingPaymentCommand } from 'app/api/api';
import { EventService } from '../../events/event.service';

@Injectable({
  providedIn: 'root'
})
export class CreateAssignPaymentService
{
  constructor(private api: Api,
    private eventService: EventService) { }

  create(command: CreateAndAssignIncomingPaymentCommand)
  {
    this.api.createAndAssignIncomingPayment_Command({ eventId: this.eventService.selectedId, ...command })
      .subscribe();
  }
}
