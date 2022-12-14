import { Injectable } from '@angular/core';
import { Api, CancelRegistrationCommand } from 'app/api/api';

@Injectable({
  providedIn: 'root'
})
export class CancelRegistrationService 
{
  constructor(private api: Api) { }

  cancelRegistration(command: CancelRegistrationCommand)
  {
    this.api.cancelRegistration_Command(command)
      .subscribe();
  }
}
