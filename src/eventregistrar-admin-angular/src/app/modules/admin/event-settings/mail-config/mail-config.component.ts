import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MAT_MOMENT_DATE_ADAPTER_OPTIONS, MomentDateAdapter } from '@angular/material-moment-adapter';
import { MatButtonModule } from '@angular/material/button';
import { DateAdapter, MAT_DATE_FORMATS, MAT_DATE_LOCALE, MatOptionModule } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { TranslateModule } from '@ngx-translate/core';
import { ExternalMailConfigurationDisplayItem, ExternalMailConfigurationUpdateItem } from 'app/api/api';
import { FuseAlertModule } from '@fuse/components/alert';
import { Subject, merge, takeUntil } from 'rxjs';
import { v4 as createUuid } from 'uuid';
import { MailConfigService } from './mail-config.service';

// See the Moment.js docs for the meaning of these formats:
// https://momentjs.com/docs/#/displaying/format/
export const DE_FORMATS = {
  parse: {
    dateInput: 'DD.MM.YYYY',
  },
  display: {
    dateInput: 'DD.MM.YYYY',
    monthYearLabel: 'DD.MM.YYYY',
    dateA11yLabel: 'DD.MM.YYYY',
    monthYearA11yLabel: 'DD.MM.YYYY',
  },
};

@Component({
  selector: 'app-mail-config',
  templateUrl: './mail-config.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    // `MomentDateAdapter` can be automatically provided by importing `MomentDateModule` in your
    // application's root module. We provide it at the component level here, due to limitations of
    // our example generation script.
    {
      provide: DateAdapter,
      useClass: MomentDateAdapter,
      deps: [MAT_DATE_LOCALE, MAT_MOMENT_DATE_ADAPTER_OPTIONS],
    },
    { provide: MAT_DATE_FORMATS, useValue: DE_FORMATS }
  ],
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatDatepickerModule,
    FuseAlertModule
  ]
})
export class MailConfigComponent implements OnInit
{
  public configForms: FormGroup[] = [];
  public submittable: boolean;
  private unsubscribeAll: Subject<any> = new Subject<any>();

  constructor(private mailConfigService: MailConfigService,
    private fb: FormBuilder,
    private changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void
  {
    this.mailConfigService.mailConfigs$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((configs: ExternalMailConfigurationDisplayItem[]) =>
      {
        this.configForms = configs.map(cfg => this.fb.group(
          {
            id: cfg.id,
            imapHost: cfg.imapHost,
            imapPort: cfg.imapPort,
            username: cfg.username,
            password: null as string,
            importMailsSince: cfg.importMailsSince,

            checkSuccessful: cfg.checkSuccessful,
            checkError: cfg.checkError,
            totalImportedMails: cfg.totalImportedMails,
            totalAssignedMails: cfg.totalAssignedMails
          }));
        this.changeDetectorRef.markForCheck();
      });
    merge(...this.configForms.map(frm => frm.statusChanges))
      .subscribe((_) =>
      {
        this.submittable = this.configForms.some(frm => frm.valid && frm.dirty);
        this.changeDetectorRef.markForCheck();
      });
  }

  addImap(): void
  {
    const newForm = this.fb.group({
      id: createUuid(),
      imapHost: null as string,
      imapPort: null as number,
      username: null as string,
      password: null as string,
      importMailsSince: null as Date,
      totalImportedMails: 0,
      totalAssignedMails: 0
    });
    this.configForms.push(newForm);
    newForm.statusChanges.subscribe((_) =>
    {
      this.submittable = this.configForms.some(frm => frm.valid && frm.dirty);
      this.changeDetectorRef.markForCheck();
    });
    this.changeDetectorRef.markForCheck();
  }

  removeImap(index: number): void
  {
    this.configForms.splice(index, 1);
  }

  save(): void
  {
    const configs = this.configForms.map(cfg => ({ ...cfg.value } as ExternalMailConfigurationUpdateItem));
    this.mailConfigService.save(configs);
  }
}
