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

  goToRegistration(registrationId: string): void
  {
    this.router.navigate(['/', this.eventService.selected.acronym, 'registrations', registrationId]);
  }

  goToSettlePaymentUrl(registrationId: string): void
  {
    this.router.navigate([this.getSettlePaymentUrl(registrationId)]);
  }

  getRegistrableUrl(registrableId: string, type: RegistrableType): string
  {
    return `/${this.eventService.selected.acronym}/overview/${registrableId}/${type === RegistrableType.Single ? 'single' : 'double'}/participants`;
  }

  goToRegistrable(registrableId: string, type: RegistrableType): void
  {
    this.router.navigate(['/', this.eventService.selected.acronym, 'overview', registrableId, type === RegistrableType.Single ? 'single' : 'double', 'participants']);
  }

  getDuePaymentUrl(): string
  {
    return `/${this.eventService.selected.acronym}/accounting/due-payments`;
  }

  getFormMappingUrl(): string
  {
    return `/${this.eventService.selected.acronym}/admin/form-mapping`;
  }

  getOverviewUrl(): string
  {
    return `/${this.eventService.selected.acronym}/overview`;
  }

  getSettlePaymentUrl(paymentId: string): string
  {
    return `/${this.eventService.selected.acronym}/accounting/settle-payments/${paymentId}`;
  }

  getPricingUrl(): string
  {
    return `/${this.eventService.selected.acronym}/admin/pricing`;
  }

  getAutoMailTemplatesUrl(): string
  {
    return `/${this.eventService.selected.acronym}/mailing/auto-mail-templates`;
  }
}
