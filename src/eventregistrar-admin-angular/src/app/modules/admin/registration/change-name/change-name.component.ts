import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { ChangeNameService } from './change-name.service';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-change-name',
  templateUrl: './change-name.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, MatFormFieldModule, MatInputModule, MatDialogModule, TranslateModule]
})
export class ChangeNameComponent
{

  public firstName: string;
  public lastName: string;

  constructor(private changeDetectorRef: ChangeDetectorRef,
    @Inject(MAT_DIALOG_DATA) private data: { registrationId: string; oldFirstName: string; oldLastName: string; },
    private service: ChangeNameService,
    public matDialogRef: MatDialogRef<ChangeNameComponent>) { }

  ngOnInit(): void
  {
    this.firstName = this.data.oldFirstName;
    this.lastName = this.data.oldLastName;
    this.changeDetectorRef.markForCheck();
  }

  change(firstName: string, lastName: string)
  {
    this.service.cancelRegistration(this.data.registrationId, firstName, lastName);
  }
}
