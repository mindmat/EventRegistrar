import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { FallbackPricePackage, IndividualReductionType, MailDisplayItem, MailDisplayType, MailMetadata, MailState, MailTypeItem, RegistrationDisplayItem, RegistrationState, SpotDisplayItem } from 'app/api/api';
import { BehaviorSubject, debounceTime, filter, Subject, switchMap, takeUntil, tap } from 'rxjs';
import { EventService } from '../events/event.service';
import { MailService } from '../mailing/mails/mail-view/mail.service';
import { NavigatorService } from '../navigator.service';
import { RemarksOverviewService } from '../registrations/remarks-overview/remarks-overview.service';
import { CancelRegistrationComponent } from './cancel-registration/cancel-registration.component';
import { ChangeEmailComponent } from './change-email/change-email.component';
import { ChangeSpotsComponent } from './change-spots/change-spots.component';
import { CreateIndividualReductionComponent } from './create-individual-reduction/create-individual-reduction.component';
import { IndividualReductionService } from './create-individual-reduction/individual-reduction.service';
import { RegistrationService } from './registration.service';
import { FallbackPackagesService } from '../pricing/fallback-packages.service';
import { CreateAssignPaymentComponent } from './create-assign-payment/create-assign-payment.component';
import { ChangeNameComponent } from './change-name/change-name.component';
import { MatchPartnerService } from '../registrations/match-partner/match-partner.service';

@Component({
  selector: 'app-registration',
  templateUrl: './registration.component.html'
})
export class RegistrationComponent implements OnInit
{
  public registration: RegistrationDisplayItem;
  public possibleMailTypes: MailTypeItem[];
  public mails: MailMetadata[];
  public notes: string | null = null;
  public notesDirty: boolean;
  public notesVersion: number;
  changeSpotsDialog: MatDialogRef<ChangeSpotsComponent> | null;
  public lastSentNotes: string | null;
  notesToSave$: BehaviorSubject<string | null> = new BehaviorSubject<string | null>(null);

  IndividualReductionType = IndividualReductionType;
  MailState = MailState;
  RegistrationState = RegistrationState;
  MailDisplayType = MailDisplayType;
  possibleFallbackPricePackages: FallbackPricePackage[];
  private unsubscribeAll: Subject<any> = new Subject<any>();

  constructor(
    private registrationService: RegistrationService,
    public navigator: NavigatorService,
    private changeDetectorRef: ChangeDetectorRef,
    private eventService: EventService,
    private matDialog: MatDialog,
    private mailService: MailService,
    private reductionService: IndividualReductionService,
    private remarksService: RemarksOverviewService,
    private fallbackPackagesService: FallbackPackagesService,
    private matchPartnerService: MatchPartnerService) { }

  ngOnInit(): void
  {
    // Get the participants
    this.registrationService.registration$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((registration: RegistrationDisplayItem) =>
      {
        this.registration = registration;
        if (!!this.changeSpotsDialog)
        {
          this.changeSpotsDialog.componentInstance.updateSpots(this.registration.spots);
        }
        if (!this.lastSentNotes
          && this.notes !== registration.internalNotes)
        {
          this.notes = registration.internalNotes;
        }
        this.mails = this.registration.mails;
        if (!!this.registration.importedMails)
        {
          this.mails = this.mails.concat(this.registration.importedMails);
        }
        this.mails = this.mails.sort(mail => (mail.sentAt ?? mail.created).valueOf());

        this.possibleMailTypes = null;

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });

    this.notesToSave$.pipe(
      debounceTime(500),
      filter(notes => notes !== null && notes !== undefined),
      switchMap((notes) =>
      {
        this.lastSentNotes = notes;
        this.changeDetectorRef.markForCheck();
        return this.registrationService.updateNotes(this.registration.id, notes);
      }),
      tap((savedNotes) =>
      {
        if (this.lastSentNotes === savedNotes)
        {
          this.lastSentNotes = null;
          this.changeDetectorRef.markForCheck();
        }
      }))
      .subscribe();
  }

  fetchPossibleMailTypes(): void
  {
    if (!this.possibleMailTypes)
    {
      this.registrationService.getPossibleMailTypes(this.registration.id)
        .subscribe(types => this.possibleMailTypes = types);
    }
  }

  fetchPossibleFalbackPackages(): void
  {
    if (!this.possibleFallbackPricePackages)
    {
      this.fallbackPackagesService.getPossiblePackages(this.registration.id)
        .subscribe(types => this.possibleFallbackPricePackages = types);
    }
  }

  createMail(mailTypeItem: MailTypeItem): void
  {
    if (mailTypeItem.type)
    {
      this.registrationService.createAutoMail(this.registration.id, mailTypeItem.type)
        .subscribe();
    }
  }

  viewMail(mailId: string): void
  {
    const url = `${this.eventService.selected.acronym}/mail-viewer/${mailId}`;
    window.open(url, '_blank', 'location=yes,height=1000,width=800,scrollbars=yes,status=yes'); // Open new window
  }

  getRegistrableUrl(spot: SpotDisplayItem): string
  {
    return this.navigator.getRegistrableUrl(spot.registrableId, spot.type);
  }

  addReduction(): void
  {
    this.matDialog.open(CreateIndividualReductionComponent, {
      autoFocus: true,
      data: { registrationId: this.registration.id, price: this.registration.price }
    });
  }

  removeReduction(reductionId: string): void
  {
    this.reductionService.removeReduction(reductionId);
  }

  cancelRegistration(): void
  {
    this.matDialog.open(CancelRegistrationComponent, {
      autoFocus: true,
      data: { registrationId: this.registration.id, paid: this.registration.paid }
    });
  }

  changeName(): void
  {
    this.matDialog.open(ChangeNameComponent, {
      autoFocus: true,
      data: { registrationId: this.registration.id, oldFirstName: this.registration.firstName, oldLastName: this.registration.lastName }
    });
  }

  changeEmail(): void
  {
    this.matDialog.open(ChangeEmailComponent, {
      autoFocus: true,
      data: { registrationId: this.registration.id, oldEmailAddress: this.registration.email }
    });
  }

  convertToSingleRegistration(): void
  {
    this.matchPartnerService.transformToSingle(this.registration.id);
  }

  addManualPayment(): void
  {
    this.matDialog.open(CreateAssignPaymentComponent, {
      autoFocus: true,
      data: { registrationId: this.registration.id, price: this.registration.price }
    });
  }

  unassignPayment(paymentAssignmentId: string): void
  {
    this.registrationService.unassignPayment(paymentAssignmentId);
  }

  changeSpots(): void
  {
    this.changeSpotsDialog = this.matDialog.open(ChangeSpotsComponent, {
      autoFocus: true,
      data: { registrationId: this.registration.id, spots: this.registration.spots }
    });
  }

  deleteMail(mailId: string): void
  {
    this.mailService.deleteMail(mailId);
  }

  releaseMail(mailId: string): void
  {
    this.mailService.releaseMail(mailId);
  }

  processedChanged(registrationId: string, processed: boolean): void
  {
    this.remarksService.setProcessedState(registrationId, processed);
  }

  notesChanged(notes: string): void
  {
    this.notesToSave$.next(notes);
  }

  unbindPartnerRegistrations(): void
  {
    this.registrationService.unbindPartnerRegistrations(this.registration.id);
  }

  setWillPayAtCheckin(): void
  {
    this.registrationService.setWillPayAtCheckin(this.registration.id);
  }


  toggleManualFallbackPackage(pricePackageId: string, remove: boolean): void
  {
    let newFallbackPackages = this.registration.pricePackageIds_ManualFallback;
    if (remove)
    {
      newFallbackPackages = [...this.registration.pricePackageIds_ManualFallback, pricePackageId];
    }
    else
    {
      const index = this.registration.pricePackageIds_ManualFallback.indexOf(pricePackageId, 0);
      if (index > -1)
      {
        newFallbackPackages = this.registration.pricePackageIds_ManualFallback.splice(index, 1);
      }
    }
    this.registrationService.setFallbackPackage(this.registration.id, newFallbackPackages);
  }

  removeFallbackPackage(): void
  {
    this.registrationService.setFallbackPackage(this.registration.id, null);
  }
}
