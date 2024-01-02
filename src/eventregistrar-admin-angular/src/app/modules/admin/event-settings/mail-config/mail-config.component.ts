import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ExternalMailConfigurationDisplayItem, ExternalMailConfigurationUpdateItem, PricePackageDto } from 'app/api/api';
import { Subject, merge, takeUntil } from 'rxjs';
import { MailConfigService } from './mail-config.service';
import { v4 as createUuid } from 'uuid';

@Component({
  selector: 'app-mail-config',
  templateUrl: './mail-config.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
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
      importMailsSince: null as Date
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
