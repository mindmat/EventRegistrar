import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ExternalMailConfigurationDisplayItem, ExternalMailConfigurationUpdateItem, PricePackageDto } from 'app/api/api';
import { Subject, takeUntil } from 'rxjs';
import { MailConfigService } from './mail-config.service';

@Component({
  selector: 'app-mail-config',
  templateUrl: './mail-config.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MailConfigComponent implements OnInit
{
  public configForms: FormGroup[] = [];
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
            imapHost: cfg.imapHost,
            imapPort: cfg.imapPort,
            username: cfg.username,
            password: null as string
          }));

        this.changeDetectorRef.markForCheck();
      });
  }

  save(): void
  {
    const configs = this.configForms.map(cfg => ({ ...cfg.value } as ExternalMailConfigurationUpdateItem));
    this.mailConfigService.save(configs);
  }
}
