import { Injectable } from '@angular/core';
import { AddIndividualReductionCommand, Api } from 'app/api/api';
import { EventService } from '../../events/event.service';

@Injectable({
  providedIn: 'root'
})
export class IndividualReductionService
{
  constructor(private api: Api,
    private eventService: EventService) { }

  addReduction(command: AddIndividualReductionCommand)
  {
    this.api.addIndividualReduction_Command(command)
      .subscribe();
  }

  removeReduction(reductionId: string)
  {
    this.api.removeIndividualReduction_Command({ eventId: this.eventService.selectedId, reductionId })
      .subscribe();
  }
}
