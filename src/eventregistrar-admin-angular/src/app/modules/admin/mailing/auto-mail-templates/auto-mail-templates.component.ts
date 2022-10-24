import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AutoMailTemplates, AutoMailTemplateMetadataLanguage, AutoMailTemplateMetadataType } from 'app/api/api';
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
  availableLangs = [
    { id: 'de', label: 'Deutsch' },
    { id: 'en', label: 'English' }
  ];

  configForm = this.fb.group({
    eventId: '',
    senderName: '',
    senderMail: '',
    singleRegistrationPossible: false,
    partnerRegistrationPossible: false,
    availableLanguages: this.fb.array([] as string[])
  });

  constructor(
    private service: AutoMailTemplatesService,
    private route: ActivatedRoute,
    private router: Router,
    private changeDetectorRef: ChangeDetectorRef,
    private fb: FormBuilder) { }

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
        this.configForm.patchValue({
          eventId: templates.eventId,
          senderName: templates.senderAlias,
          senderMail: templates.senderMail,
          singleRegistrationPossible: templates.singleRegistrationPossible,
          partnerRegistrationPossible: templates.partnerRegistrationPossible,
          availableLanguages: []
        });
        this.configForm.setControl('availableLanguages', this.fb.array(templates.availableLanguages));

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });

  }

  toggleLang(langId: string)
  {
    var langs = this.configForm.value.availableLanguages;
    var index = langs.indexOf(langId);
    if (index < 0)
    {
      langs.push(langId);
    }
    else
    {
      langs.splice(index, 1);
    }
  }

  toggleSingle()
  {
    this.configForm.patchValue({
      singleRegistrationPossible: !this.configForm.value.singleRegistrationPossible
    });
  }

  togglePartner()
  {
    this.configForm.patchValue({
      partnerRegistrationPossible: !this.configForm.value.partnerRegistrationPossible
    });
  }

  toggleRelease(type: AutoMailTemplateMetadataType)
  {
    type.releaseImmediately = !type.releaseImmediately;
    this.service.setReleaseMail(type.type, type.releaseImmediately);
  }

  selectTemplate(template: AutoMailTemplateMetadataLanguage, type: AutoMailTemplateMetadataType)
  {
    this.selectedTemplate = template;

    if (template.id == null)
    {
      this.service.createTemplate(type.type, template.language)
        .subscribe(id => this.router.navigate([`./${id}`], { relativeTo: this.route }));
    }
    else
    {
      this.router.navigate([`./${template.id}`], { relativeTo: this.route });
    }

    // Mark for check
    this.changeDetectorRef.markForCheck();
  }

  saveSettings()
  {
    this.service.updateSettings(
      this.configForm.value.senderMail,
      this.configForm.value.senderName,
      this.configForm.value.availableLanguages,
      this.configForm.value.singleRegistrationPossible,
      this.configForm.value.partnerRegistrationPossible);
  }
}
