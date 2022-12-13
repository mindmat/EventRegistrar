import { Injectable } from '@angular/core';
import { AddIndividualReductionCommand, Api } from 'app/api/api';

@Injectable({
  providedIn: 'root'
})
export class IndividualReductionService
{
  constructor(private api: Api) { }

  addReduction(command: AddIndividualReductionCommand)
  {
    this.api.addIndividualReduction_Command(command)
      .subscribe();
  }
}
