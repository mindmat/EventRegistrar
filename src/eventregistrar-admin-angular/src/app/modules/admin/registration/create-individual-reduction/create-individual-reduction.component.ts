import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject, OnInit } from '@angular/core';
import { FormGroup, FormBuilder } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { AddIndividualReductionCommand, IndividualReductionType } from 'app/api/api';
import { EventService } from '../../events/event.service';
import { RegistrableDetailComponent } from '../../overview/registrable-detail/registrable-detail.component';
import { v4 as createUuid } from 'uuid';
import { IndividualReductionService } from './individual-reduction.service';

@Component({
  selector: 'app-create-individual-reduction',
  templateUrl: './create-individual-reduction.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
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
