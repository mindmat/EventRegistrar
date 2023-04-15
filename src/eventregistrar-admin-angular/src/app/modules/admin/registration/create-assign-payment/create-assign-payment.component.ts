import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject } from '@angular/core';
import { FormGroup, FormBuilder } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { IndividualReductionType, CreateAndAssignIncomingPaymentCommand } from 'app/api/api';
import { EventService } from '../../events/event.service';
import { v4 as createUuid } from 'uuid';
import { CreateAssignPaymentService } from './create-assign-payment.service';

@Component({
  selector: 'app-create-assign-payment',
  templateUrl: './create-assign-payment.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CreateAssignPaymentComponent
{
  form: FormGroup;
  IndividualReductionType = IndividualReductionType;

  constructor(private changeDetectorRef: ChangeDetectorRef,
    @Inject(MAT_DIALOG_DATA) private data: { registrationId: string; price?: number; },
    private service: CreateAssignPaymentService,
    public matDialogRef: MatDialogRef<CreateAssignPaymentComponent>,
    private fb: FormBuilder,
    private eventService: EventService) { }

  ngOnInit(): void
  {
    this.form = this.fb.group<CreateAndAssignIncomingPaymentCommand>({
      eventId: this.eventService.selectedId,
      registrationId: this.data.registrationId,
      paymentId: createUuid(),
      amount: this.data.price,
      debitorIban: null,
      debitorName: null,
      bookingDate: new Date(),
      message: null
    });

    this.changeDetectorRef.markForCheck();
  };

  onSubmit(): void
  {
    var command = this.form.value as CreateAndAssignIncomingPaymentCommand;
    this.service.create(command);
  };
}
