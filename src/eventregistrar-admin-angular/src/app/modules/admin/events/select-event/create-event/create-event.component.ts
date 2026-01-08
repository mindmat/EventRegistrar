import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CreateEventCommand, EventOfUser } from 'app/api/api';
import { v4 as createUuid } from 'uuid';
import { CreateEventService } from './create-event.service';

@Component({
  selector: 'app-create-event',
  templateUrl: './create-event.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatCheckboxModule
  ]
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
    copyConfigurations: true,
    copyBulkMailTemplates: true,
    copyPricing: true
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

  create(): void
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
