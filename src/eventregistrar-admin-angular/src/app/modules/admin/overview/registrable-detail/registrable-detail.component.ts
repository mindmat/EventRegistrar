import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { RegistrableDisplayItem, RegistrableType, RegistrableTypeOption, SaveRegistrableCommand } from 'app/api/api';
import { EventService } from '../../events/event.service';
import { RegistrablesService } from '../../pricing/registrables.service';
import { v4 as createUuid } from 'uuid';

@Component({
  selector: 'app-registrable-detail',
  templateUrl: './registrable-detail.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RegistrableDetailComponent
{
  registrableForm: FormGroup;
  registrableTypes: RegistrableTypeOption[];
  registrableType = RegistrableType;
  constructor(private changeDetectorRef: ChangeDetectorRef,
    @Inject(MAT_DIALOG_DATA) private data: { registrable?: RegistrableDisplayItem; },
    private registrablesService: RegistrablesService,
    public matDialogRef: MatDialogRef<RegistrableDetailComponent>,
    private fb: FormBuilder,
    private eventService: EventService) { }

  ngOnInit(): void
  {
    this.registrableForm = this.fb.group<SaveRegistrableCommand & { hasSingleMax: boolean; hasDoubleMax: boolean; }>({
      eventId: this.eventService.selectedId,
      registrableId: createUuid(),
      name: '',
      nameSecondary: null,
      tag: null,
      hasWaitingList: false,
      type: RegistrableType.Single,

      hasSingleMax: false,
      maximumSingleSpots: null,

      hasDoubleMax: false,
      maximumDoubleSpots: null,
      maximumAllowedImbalance: null
    });
    this.changeDetectorRef.markForCheck();

    this.registrablesService.getRegistrableTypes().subscribe(types =>
    {
      this.registrableTypes = types;
      this.changeDetectorRef.markForCheck();
    });
  }

  onSubmit(): void
  {
    var command = { ... this.registrableForm.value } as SaveRegistrableCommand;
    if (command.type == RegistrableType.Single)
    {
      if (!this.registrableForm.value.hasSingleMax)
      {
        command.maximumSingleSpots = null;
      }
      command.maximumDoubleSpots = null;
      command.maximumAllowedImbalance = null;
      command.hasWaitingList &&= command.maximumSingleSpots != null;
    }
    else if (command.type == RegistrableType.Double)
    {
      if (!this.registrableForm.value.hasDoubleMax)
      {
        command.maximumDoubleSpots = null;
      }
      command.maximumSingleSpots = null;
      command.hasWaitingList &&= command.maximumDoubleSpots != null;
    }
    this.registrablesService.saveRegistrable(command);
  }

  canHaveWaitingList(): boolean
  {
    return this.registrableForm.value.type == RegistrableType.Double && this.registrableForm.value.hasDoubleMax
      || this.registrableForm.value.type == RegistrableType.Single && this.registrableForm.value.hasSingleMax;
  }
}
