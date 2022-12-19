import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { IndividualReductionType, MailState, MailTypeItem, RegistrationDisplayItem, SpotDisplayItem } from 'app/api/api';
import { Subject, takeUntil } from 'rxjs';
import { EventService } from '../events/event.service';
import { MailService } from '../mailing/mails/mail-view/mail.service';
import { NavigatorService } from '../navigator.service';
import { CancelRegistrationComponent } from './cancel-registration/cancel-registration.component';
import { ChangeEmailComponent } from './change-email/change-email.component';
import { ChangeSpotsComponent } from './change-spots/change-spots.component';
import { CreateIndividualReductionComponent } from './create-individual-reduction/create-individual-reduction.component';
import { IndividualReductionService } from './create-individual-reduction/individual-reduction.service';
import { RegistrationService } from './registration.service';

@Component({
  selector: 'app-registration',
  templateUrl: './registration.component.html'
})
export class RegistrationComponent implements OnInit
{
  public registration: RegistrationDisplayItem;
  private unsubscribeAll: Subject<any> = new Subject<any>();
  public possibleMailTypes: MailTypeItem[];
  IndividualReductionType = IndividualReductionType;
  MailState = MailState;
  changeSpotsDialog: MatDialogRef<ChangeSpotsComponent> | null;

  constructor(
    private service: RegistrationService,
    public navigator: NavigatorService,
    private changeDetectorRef: ChangeDetectorRef,
    private eventService: EventService,
    private matDialog: MatDialog,
    private mailService: MailService,
    private reductionService: IndividualReductionService) { }

  ngOnInit(): void
  {
    // Get the participants
    this.service.registration$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((registration: RegistrationDisplayItem) =>
      {
        this.registration = registration;
        if (!!this.changeSpotsDialog)
        {
          this.changeSpotsDialog.componentInstance.updateSpots(this.registration.spots);
        }

        this.possibleMailTypes = null;

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });
  }

  fetchPossibleMailTypes()
  {
    if (!this.possibleMailTypes)
    {
      this.service.getPossibleMailTypes(this.registration.id)
        .subscribe(types => this.possibleMailTypes = types);
    }
  }

  createMail(mailTypeItem: MailTypeItem)
  {
    if (mailTypeItem.type)
    {
      this.service.createAutoMail(this.registration.id, mailTypeItem.type)
        .subscribe();
    }
  }

  viewMail(mailId: string)
  {
    var url = `${this.eventService.selected.acronym}/mail-viewer/${mailId}`;
    window.open(url, '_blank', 'location=yes,height=1000,width=800,scrollbars=yes,status=yes'); // Open new window
  }

  getRegistrableUrl(spot: SpotDisplayItem): string
  {
    return this.navigator.getRegistrableUrl(spot.registrableId, spot.type);
  }

  addReduction()
  {
    this.matDialog.open(CreateIndividualReductionComponent, {
      autoFocus: true,
      data: { registrationId: this.registration.id, price: this.registration.price }
    });
  }

  removeReduction(reductionId: string)
  {
    this.reductionService.removeReduction(reductionId);
  }

  cancelRegistration()
  {
    this.matDialog.open(CancelRegistrationComponent, {
      autoFocus: true,
      data: { registrationId: this.registration.id, paid: this.registration.paid }
    });
  }

  changeEmail()
  {
    this.matDialog.open(ChangeEmailComponent, {
      autoFocus: true,
      data: { registrationId: this.registration.id, oldEmailAddress: this.registration.email }
    });
  }

  changeSpots()
  {
    this.changeSpotsDialog = this.matDialog.open(ChangeSpotsComponent, {
      autoFocus: true,
      data: { registrationId: this.registration.id, spots: this.registration.spots }
    });
  }

  deleteMail(mailId: string)
  {
    this.mailService.deleteMail(mailId);
  }

  releaseMail(mailId: string)
  {
    this.mailService.releaseMail(mailId);
  }
}
