import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { DoubleRegistrableDisplayItem, RegistrableType, RegistrableTypeOption, SaveRegistrableCommand, SingleRegistrableDisplayItem } from 'app/api/api';
import { EventService } from '../../events/event.service';
import { RegistrablesService } from '../../pricing/registrables.service';
import { v4 as createUuid } from 'uuid';

@Component({
  selector: 'app-registrable-detail',
  templateUrl: './registrable-detail.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RegistrableDetailComponent implements OnInit
{
  registrableForm: FormGroup;
  registrableTypes: RegistrableTypeOption[];
  registrableType = RegistrableType;

  constructor(private changeDetectorRef: ChangeDetectorRef,
    @Inject(MAT_DIALOG_DATA) private data: { singleRegistrable?: SingleRegistrableDisplayItem; doubleRegistrable?: DoubleRegistrableDisplayItem; },
    private registrablesService: RegistrablesService,
    public matDialogRef: MatDialogRef<RegistrableDetailComponent>,
    private fb: FormBuilder,
    private eventService: EventService) { }

  ngOnInit(): void
  {
    if (this.data.singleRegistrable != null)
    {
      this.registrableForm = this.fb.group<SaveRegistrableCommand & { hasSingleMax: boolean; hasDoubleMax: boolean; }>({
        eventId: this.eventService.selectedId,
        registrableId: this.data.singleRegistrable.id,
        name: this.data.singleRegistrable.name,
        nameSecondary: this.data.singleRegistrable.nameSecondary,
        tag: this.data.singleRegistrable.tag,
        hasWaitingList: this.data.singleRegistrable.hasWaitingList,
        type: RegistrableType.Single,
        isCore: this.data.singleRegistrable.isCore,
        checkinListColumn: this.data.singleRegistrable.checkinListColumn,

        hasSingleMax: this.data.singleRegistrable.spotsAvailable != null,
        maximumSingleSpots: this.data.singleRegistrable.spotsAvailable,

        hasDoubleMax: false,
        maximumDoubleSpots: null,
        maximumAllowedImbalance: null
      });
    }
    else if (this.data.doubleRegistrable != null)
    {
      this.registrableForm = this.fb.group<SaveRegistrableCommand & { hasSingleMax: boolean; hasDoubleMax: boolean; }>({
        eventId: this.eventService.selectedId,
        registrableId: this.data.doubleRegistrable.id,
        name: this.data.doubleRegistrable.name,
        nameSecondary: this.data.doubleRegistrable.nameSecondary,
        tag: this.data.doubleRegistrable.tag,
        hasWaitingList: this.data.doubleRegistrable.hasWaitingList,
        type: RegistrableType.Double,
        isCore: this.data.doubleRegistrable.isCore,
        checkinListColumn: this.data.doubleRegistrable.checkinListColumn,

        hasSingleMax: false,
        maximumSingleSpots: null,

        hasDoubleMax: this.data.doubleRegistrable.spotsAvailable != null,
        maximumDoubleSpots: this.data.doubleRegistrable.spotsAvailable,
        maximumAllowedImbalance: this.data.doubleRegistrable.maximumAllowedImbalance
      });
    }
    else
    {
      // new
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
        maximumAllowedImbalance: null,

        isCore: false,
        checkinListColumn: null
      });
    }

    this.changeDetectorRef.markForCheck();

    this.registrablesService.getRegistrableTypes().subscribe((types) =>
    {
      this.registrableTypes = types;
      this.changeDetectorRef.markForCheck();
    });
  }

  onSubmit(): void
  {
    const command = { ... this.registrableForm.value } as SaveRegistrableCommand;
    if (command.type === RegistrableType.Single)
    {
      if (!this.registrableForm.value.hasSingleMax)
      {
        command.maximumSingleSpots = null;
      }
      command.maximumDoubleSpots = null;
      command.maximumAllowedImbalance = null;
      command.hasWaitingList &&= command.maximumSingleSpots != null;
    }
    else if (command.type === RegistrableType.Double)
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
    return this.registrableForm.value.type === RegistrableType.Double && this.registrableForm.value.hasDoubleMax
      || this.registrableForm.value.type === RegistrableType.Single && this.registrableForm.value.hasSingleMax;
  }
}
