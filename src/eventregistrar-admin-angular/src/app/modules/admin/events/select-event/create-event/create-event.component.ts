import { ChangeDetectionStrategy, Component, Inject, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { CreateEventCommand, EventOfUser } from 'app/api/api';
import { CreateEventService } from './create-event.service';
import { v4 as createUuid } from 'uuid';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-create-event',
  templateUrl: './create-event.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CreateEventComponent implements OnInit
{
  createEventForm = this.fb.group<CreateEventCommand>({
    id: createUuid(),
    eventId_Predecessor: '',
    name: '',
    acronym: '',
    copyAccessRights: true,
    copyRegistrables: true,
    copyAutoMailTemplates: true,
    copyConfigurations: true
  });
  public title: string;

  constructor(public matDialogRef: MatDialogRef<CreateEventComponent>,
    private fb: FormBuilder,
    private service: CreateEventService,
    private translateService: TranslateService,
    @Inject(MAT_DIALOG_DATA) public data: { event: EventOfUser; })
  {
    if (!!data)
    {
      this.createEventForm.patchValue({
        eventId_Predecessor: data.event.eventId,
        name: data.event.eventName,
        acronym: data.event.eventAcronym
      });
      this.translateService.get('CreateSuccessorEventOf')
        .subscribe((txt: string) => this.title = txt.replace('{event}', data.event.eventName));
    }
    else
    {
      this.translateService.get('CreateSuccessorEvent')
        .subscribe((txt: string) => this.title = txt);
    }
  }

  ngOnInit(): void
  {
  }

  create()
  {
    this.service.createEvent(this.createEventForm.value);

    // Close the dialog
    this.matDialogRef.close();
  }

  discard(): void
  {
    // Close the dialog
    this.matDialogRef.close();
  }
}
