import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject } from '@angular/core';
import { FormGroup, FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { IndividualReductionType, CreateAndAssignIncomingPaymentCommand } from 'app/api/api';
import { EventService } from '../../events/event.service';
import { v4 as createUuid } from 'uuid';
import { CreateAssignPaymentService } from './create-assign-payment.service';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-create-assign-payment',
  templateUrl: './create-assign-payment.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatButtonModule, MatIconModule, MatFormFieldModule, MatInputModule, MatDatepickerModule, MatNativeDateModule, MatDialogModule, TranslateModule]
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
