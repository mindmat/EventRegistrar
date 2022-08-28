import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { AutoMailTemplates, AutoMailTemplateMetadataLanguage } from 'app/api/api';
import { Subject, takeUntil } from 'rxjs';
import { AutoMailTemplatesService } from './auto-mail-templates.service';

@Component({
  selector: 'app-auto-mail-templates',
  templateUrl: './auto-mail-templates.component.html'
})
export class AutoMailTemplatesComponent implements OnInit
{
  private unsubscribeAll: Subject<any> = new Subject<any>();
  templates: AutoMailTemplates;
  selectedTemplate: AutoMailTemplateMetadataLanguage;
  flagCodes: any;

  constructor(private service: AutoMailTemplatesService, private changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void
  {
    this.flagCodes = {
      'de': 'de',
      'en': 'us'
    };

    this.service.autoMailTemplates$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((templates: AutoMailTemplates) =>
      {
        this.templates = templates;

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });

  }

  selectTemplate(template: AutoMailTemplateMetadataLanguage)
  {
    this.selectedTemplate = template;

    // Mark for check
    this.changeDetectorRef.markForCheck();
  }

}
