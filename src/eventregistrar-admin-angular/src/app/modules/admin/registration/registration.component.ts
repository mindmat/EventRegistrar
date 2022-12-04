import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { MailType, MailTypeItem, RegistrationDisplayItem, SpotDisplayInfo, SpotDisplayItem } from 'app/api/api';
import { Subject, takeUntil } from 'rxjs';
import { NavigatorService } from '../navigator.service';
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

  constructor(
    private service: RegistrationService,
    public navigator: NavigatorService,
    private changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void
  {
    // Get the participants
    this.service.registration$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((registration: RegistrationDisplayItem) =>
      {
        this.registration = registration;

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

  getRegistrableUrl(spot: SpotDisplayItem): string
  {
    return this.navigator.getRegistrableUrl(spot.registrableId, spot.type);
  }
}
