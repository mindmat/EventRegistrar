import { Injectable } from '@angular/core';
import { Api, CreateEventCommand } from 'app/api/api';

@Injectable({
  providedIn: 'root'
})
export class CreateEventService
{
  constructor(private api: Api) { }

  createEvent(command: CreateEventCommand)
  {
    this.api.createEvent_Command(command)
      .subscribe();
  }
}
