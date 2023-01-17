import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { RegistrableType } from 'app/api/api';
import { EventService } from './events/event.service';

@Injectable({
  providedIn: 'root'
})
export class NavigatorService
{
  constructor(private router: Router,
    private eventService: EventService) { }

  getRegistrationUrl(registrationId: string): string
  {
    return `/${this.eventService.selected.acronym}/registrations/${registrationId}`;
  }

  goToRegistration(registrationId: string)
  {
    this.router.navigate(['/', this.eventService.selected.acronym, 'registrations', registrationId]);
  }

  getRegistrableUrl(registrableId: string, type: RegistrableType): string
  {
    return `/${this.eventService.selected.acronym}/overview/${registrableId}/${type == RegistrableType.Single ? 'single' : 'double'}/participants`;
  }

  goToRegistrable(registrableId: string, type: RegistrableType)
  {
    this.router.navigate(['/', this.eventService.selected.acronym, 'overview', registrableId, type == RegistrableType.Single ? 'single' : 'double', 'participants']);
  }

  getDuePaymentUrl(): string
  {
    return `/${this.eventService.selected.acronym}/accounting/due-payments`;
  }
}
