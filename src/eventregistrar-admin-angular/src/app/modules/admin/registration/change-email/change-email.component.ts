import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject } from '@angular/core';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { EventService } from '../../events/event.service';
import { ChangeEmailService } from './change-email.service';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-change-email',
  templateUrl: './change-email.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, MatFormFieldModule, MatInputModule, MatDialogModule, TranslateModule]
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
