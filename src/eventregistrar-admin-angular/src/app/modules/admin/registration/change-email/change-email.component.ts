import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { EventService } from '../../events/event.service';
import { ChangeEmailService } from './change-email.service';

@Component({
  selector: 'app-change-email',
  templateUrl: './change-email.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChangeEmailComponent
{
  public emailAddress: string;

  constructor(private changeDetectorRef: ChangeDetectorRef,
    @Inject(MAT_DIALOG_DATA) private data: { registrationId: string; oldEmailAddress: string; },
    private service: ChangeEmailService,
    public matDialogRef: MatDialogRef<ChangeEmailComponent>) { }

  ngOnInit(): void
  {
    this.emailAddress = this.data.oldEmailAddress;
    this.changeDetectorRef.markForCheck();
  }

  change(email: string)
  {
    this.service.cancelRegistration(this.data.registrationId, this.data.oldEmailAddress, email);
  }
}
