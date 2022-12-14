import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject } from '@angular/core';
import { FormGroup, FormBuilder } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { CancelRegistrationCommand } from 'app/api/api';
import { EventService } from '../../events/event.service';
import { RegistrableDetailComponent } from '../../overview/registrable-detail/registrable-detail.component';
import { CancelRegistrationService } from './cancel-registration.service';

@Component({
  selector: 'app-cancel-registration',
  templateUrl: './cancel-registration.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CancelRegistrationComponent
{
  cancellationForm: FormGroup;

  constructor(private changeDetectorRef: ChangeDetectorRef,
    @Inject(MAT_DIALOG_DATA) private data: { registrationId: string; paid?: number; },
    private cancelService: CancelRegistrationService,
    public matDialogRef: MatDialogRef<RegistrableDetailComponent>,
    private fb: FormBuilder,
    private eventService: EventService) { }

  ngOnInit(): void
  {
    this.cancellationForm = this.fb.group<CancelRegistrationCommand>({
      eventId: this.eventService.selectedId,
      registrationId: this.data.registrationId,

      reason: '',
      ignorePayments: false,
      received: new Date(),
      refundPercentage: 0
    });

    this.changeDetectorRef.markForCheck();
  }

  onSubmit(): void
  {
    var command = this.cancellationForm.value as CancelRegistrationCommand;
    this.cancelService.cancelRegistration(command);
  }
}
