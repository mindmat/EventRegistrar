import { Injectable } from '@angular/core';
import { Api, CreateEventCommand } from 'app/api/api';

@Injectable({
  providedIn: 'root'
})
export class CreateEventService
{

  constructor(private api: Api) { }

  // createEvent(id: string, name: string, acronym: string, eventId_CopyFrom: string, copyAccessRights: boolean, copyRegistrables: boolean, copyAutoMailTemplates: boolean, copyConfigurations: boolean)
  createEvent(command: CreateEventCommand)
  {
    this.api.createEvent_Command(command)
      .subscribe();
  }
}
