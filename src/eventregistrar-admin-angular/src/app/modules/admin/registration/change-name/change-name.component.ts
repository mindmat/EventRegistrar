import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { ChangeNameService } from './change-name.service';

@Component({
  selector: 'app-change-name',
  templateUrl: './change-name.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
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
