import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { AddIndividualReductionCommand, IndividualReductionType } from 'app/api/api';
import { EventService } from '../../events/event.service';
import { RegistrableDetailComponent } from '../../overview/registrable-detail/registrable-detail.component';
import { v4 as createUuid } from 'uuid';
import { IndividualReductionService } from './individual-reduction.service';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-create-individual-reduction',
  templateUrl: './create-individual-reduction.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatButtonModule, MatIconModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatDialogModule, TranslateModule]
})
export class CreateIndividualReductionComponent implements OnInit
{
  reductionForm: FormGroup;
  individualReductionType = IndividualReductionType;

  constructor(private changeDetectorRef: ChangeDetectorRef,
    @Inject(MAT_DIALOG_DATA) private data: { registrationId: string; price?: number; },
    private reductionService: IndividualReductionService,
    public matDialogRef: MatDialogRef<RegistrableDetailComponent>,
    private fb: FormBuilder,
    private eventService: EventService) { }

  ngOnInit(): void
  {
    this.reductionForm = this.fb.group<AddIndividualReductionCommand>({
      eventId: this.eventService.selectedId,
      registrationId: this.data.registrationId,

      reductionId: createUuid(),
      type: IndividualReductionType.Reduction,
      amount: 0,
      reason: ''
    });

    this.changeDetectorRef.markForCheck();
  }

  onSubmit(): void
  {
    const command = this.reductionForm.value as AddIndividualReductionCommand;
    if (command.type === IndividualReductionType.Percentage)
    {
      command.amount = Math.min(Math.max(command.amount / 100.0, 0), 1);
    }
    this.reductionService.addReduction(command);
  }
}
